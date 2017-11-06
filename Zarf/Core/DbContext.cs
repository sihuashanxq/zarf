using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Update;
using Zarf.Update.Executors;
using Zarf.Update.Compilers;
using Zarf.Builders;

namespace Zarf
{
    public abstract class DbContext : IDisposable
    {
        public DbContext(string connectionString)
        {

        }

        public virtual void Config()
        {

        }

        public static IServiceProvider ServiceProvider { get; set; }

        protected IDbCommandExecutor Executor { get; set; }

        protected IDataBaseFacade DataBase { get; set; }

        public IDbCommandFacade Command { get; protected set; }

        protected IDbService DbService { get; set; }

        public virtual IDbQuery<TEntity> Query<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider(this));
        }

        public virtual IDbQuery<TEntity> Query<TEntity>(string queryText)
        {
            return null;
        }

        public virtual void AddRange(IEnumerable<object> entities) => AddRange<object>(entities);

        public virtual void AddRange<TEntity>(IEnumerable<TEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
            {
                return;
            }

            var entries = new List<EntityEntry>();
            foreach (var entity in entities)
            {
                entries.Add(EntityEntry.Create(entity, EntityState.Insert));
            }

            Executor.Execute(new DbModifyOperation(entries, null));
        }

        public virtual void Add<TEntity>(TEntity entity) => AddRange(new[] { entity });

        public virtual void Add(object entity) => Add<object>(entity);

        public virtual int Update<TEntity>(TEntity entity) => Update<TEntity, TEntity>(entity, null);

        public virtual int Update(object entity) => Update<object>(entity);

        public virtual int Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            return Executor.Execute(new DbModifyOperation(new[] { EntityEntry.Create(entity, EntityState.Update) }, GetIdentityMember(identity)));
        }

        public virtual int Delete<TEntity>(TEntity entity) => Delete<TEntity, TEntity>(entity, null);

        public virtual int Delete(object entity) => Delete<object>(entity);

        public virtual int Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            return Executor.Execute(new DbModifyOperation(new[] { EntityEntry.Create(entity, EntityState.Delete) }, GetIdentityMember(identity)));
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

        public void Dispose()
        {

        }
    }
}
