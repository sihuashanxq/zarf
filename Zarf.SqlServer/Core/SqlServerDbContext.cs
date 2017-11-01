using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.SqlServer.Builders;

namespace Zarf
{
    public class SqlServerDbContext : DbContext
    {
        public override void Add<TEntity>(TEntity entity)
        {

        }

        public override int Update<TEntity>(TEntity entity)
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

        public override int Delete<TEntity>(TEntity entity)
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

        public override int AddRange<TEntity>(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public override int Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> keyMember)
        {
            var member = keyMember?.As<LambdaExpression>()?.Body?.As<MemberExpression>()?.Member;
            if (keyMember != null && member == null)
            {
                throw new NotImplementedException("argument keyMember must as a MemberAccessExpression!");
            }

            return 0;
        }

        public override int Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> keyMember)
        {
            throw new NotImplementedException();
        }
    }
}
