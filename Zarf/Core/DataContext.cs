using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Zarf.Mapping;

namespace Zarf
{
    public class DataContext
    {
        public DataContext()
        {
        }

        public IDataQuery<TEntity> DataQuery<TEntity>()
        {
            return new DataQuery<TEntity>(new DataQueryProvider());
        }

        public void Add<TEntity>(TEntity entity)
        {
            var entityTypeDescriptor = EntityTypeDescriptorFactory.Factory.Create(typeof(TEntity));
            var members = entityTypeDescriptor.GetReadableMembers().ToList();
            var parameterValues = new Dictionary<string, object>();
            var columns = new List<string>();

            var i = 0;
            foreach (var member in members)
            {
                if (columns.Contains(member.Name))
                {
                    continue;
                }

                columns.Add(member.Name);
                parameterValues["@p" + i] = GetMemberValue(entity, member);
                i++;
            }

            var sql = new StringBuilder();
            sql.Append(" INSERT INTO [");
            sql.Append(typeof(TEntity).Name);
            sql.Append("] (");

            foreach (var item in columns)
            {
                sql.Append(item + ",");
            }
            sql.Length--;
            sql.Append(")");
            sql.Append(" VALUES (");

            foreach (var item in parameterValues)
            {
                sql.Append(item.Key + ",");
            }

            sql.Length--;
            sql.Append(")");

            //new LinqExpressionInvoker().Add(sql.ToString(), parameterValues);
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
