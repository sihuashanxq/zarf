using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Extensions;
using Zarf.Builders;
using Zarf.Entities;

namespace Zarf
{
    public class DbParameter
    {
        public object Value { get; }

        public string Name { get; }

        public DbParameter(string name, object value)
        {
            Value = value;
            Name = name;
        }
    }

    public class DbContext
    {
        public DbContext()
        {

        }

        public IDbQuery<TEntity> DbQuery<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider());
        }

        public void Add<TEntity>(TEntity entity)
        {
            var eType = typeof(TEntity);
            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(eType);
            var table = eType.ToTable();
            var dbParams = new List<DbParameter>();
            var dbColumns = new List<string>();
            MemberInfo autoIncrement = null;

            foreach (var item in typeDescriptor.GetExpandMembers())
            {
                if (item.GetCustomAttribute<AutoIncrementAttribute>() != null)
                {
                    autoIncrement = item;
                    continue;
                }

                dbColumns.Add(item.Name);
                dbParams.Add(new DbParameter("@" + item.Name, GetMemberValue(entity, item)));
            }

            var insert = new InsertExpression(table, dbParams, dbColumns, autoIncrement != null);
            var sql = new SqlServerTextBuilder().Build(insert);
            var dbCommand = new DbCommand(string.Empty);
            if (autoIncrement == null)
            {
                dbCommand.ExecuteNonQuery(sql, dbParams.ToArray());
            }
            else
            {
                var id = dbCommand.ExecuteScalar(sql, dbParams.ToArray());
                autoIncrement.As<PropertyInfo>().SetValue(entity, int.Parse(id.ToString()));
            }
        }

        public int Update<TEntity>(TEntity entity)
        {
            var eType = typeof(TEntity);
            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(eType);
            var dbParams = new List<DbParameter>();
            var dbColumns = new List<string>();
            var byKey = "";
            DbParameter byKeyValue = null;

            foreach (var item in typeDescriptor.GetExpandMembers())
            {
                if (item.Name.ToLower() == "id")
                {
                    byKey = item.Name;
                    byKeyValue = new DbParameter("@" + item.Name, GetMemberValue(entity, item));
                    continue;
                }

                dbColumns.Add(item.Name);
                dbParams.Add(new DbParameter("@" + item.Name, GetMemberValue(entity, item)));
            }

            var update = new UpdateExpression(eType.ToTable(), dbParams, dbColumns, byKey, byKeyValue);
            var sql = new SqlServerTextBuilder().Build(update);
            var dbCommand = new DbCommand(string.Empty);
            return (int)dbCommand.ExecuteScalar(sql, dbParams.ToArray());
        }

        public int Delete<TEntity>(TEntity entity)
        {
            var eType = typeof(TEntity);
            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(eType);
            var byKey = "";
            DbParameter byKeyValue = null;

            foreach (var item in typeDescriptor.GetExpandMembers())
            {
                if (item.Name.ToLower() == "id")
                {
                    byKey = item.Name;
                    byKeyValue = new DbParameter("@" + item.Name, GetMemberValue(entity, item));
                    break;
                }
            }

            var delete = new DeleteExpression(eType.ToTable(), byKey, byKeyValue);
            var sql = new SqlServerTextBuilder().Build(delete);
            var dbCommand = new DbCommand(string.Empty);
            return (int)dbCommand.ExecuteScalar(sql, byKeyValue);
        }

        public object GetMemberValue(object instance, MemberInfo member)
        {
            if (member is PropertyInfo)
            {
                return (member as PropertyInfo).GetValue(instance);
            }

            return (member as FieldInfo).GetValue(instance);
        }
    }
}
