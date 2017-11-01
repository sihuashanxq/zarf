using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace Zarf
{
    public abstract class DbContext : IDisposable
    {
        //TODO
        public static IServiceProvider ServiceProvider { get; set; }

        public virtual IDbQuery<TEntity> Query<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider(this));
        }

        public virtual IDbQuery<TEntity> Query<TEntity>(string queryText)
        {
            return null;
        }

        public abstract int AddRange<TEntity>(IEnumerable<TEntity> entities);

        public virtual int AddRange(IEnumerable<object> entities) => AddRange<object>(entities);

        public abstract void Add<TEntity>(TEntity entity);

        public virtual void Add(object entity) => Add<object>(entity);

        public abstract int Update<TEntity>(TEntity entity);

        public virtual int Update(object entity) => Update<object>(entity);

        public abstract int Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> byKey);

        public abstract int Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> byKey);

        public abstract int Delete<TEntity>(TEntity entity);

        public virtual int Delete(object entity) => Delete<object>(entity);

        public object GetMemberValue(object instance, MemberInfo member)
        {
            if (member is PropertyInfo)
            {
                return (member as PropertyInfo).GetValue(instance);
            }

            return (member as FieldInfo).GetValue(instance);
        }

        public void Dispose()
        {

        }
    }
}
