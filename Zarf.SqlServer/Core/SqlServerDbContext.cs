using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Update;

namespace Zarf
{
    public class SqlServerDbContext : DbContext
    {
        public override int AddRange<TEntity>(IEnumerable<TEntity> entities)
        {
            var entries = new List<EntityEntry>();
            foreach (var entity in entities)
            {
                entries.Add(EntityEntry.Create(entity, EntityState.Insert));
            }

            var modifyOperation = new DbModifyOperation(entries, null);
            throw new NotImplementedException();
        }

        public override int Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            var identityMember = GetIdentityMember(identity);
            var entry = EntityEntry.Create(entity, EntityState.Update);
            var modifyOperation = new DbModifyOperation(new[] { entry }, null);

            return 0;
        }

        public override int Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            var identityMember = GetIdentityMember(identity);
            var entry = EntityEntry.Create(entity, EntityState.Update);
            var modifyOperation = new DbModifyOperation(new[] { entry }, null);

            return 0;
        }

        private static MemberDescriptor GetIdentityMember(Expression identity)
        {
            var member = identity?.As<LambdaExpression>()?.Body?.As<MemberExpression>()?.Member;
            if (identity != null && member == null)
            {
                throw new NotImplementedException("argument keyMember must as a MemberAccessExpression!");
            }

            if (member == null)
            {
                return null;
            }

            return new MemberDescriptor(member);
        }
    }
}
