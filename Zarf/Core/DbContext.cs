using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace Zarf
{
    public abstract class DbContext : IDisposable
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public virtual IDbQuery<TEntity> Query<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider(this));
        }

        public virtual IDbQuery<TEntity> Query<TEntity>(string queryText)
        {
            return null;
        }

        public abstract void AddRange<TEntity>(IEnumerable<TEntity> entities);

        public abstract int AddRange<TEntity>(IEnumerable<TEntity> entities, Expression<Func<TEntity, bool>> predicate);

        public abstract void Add<TEntity>(TEntity entity);

        public abstract int Add<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate);

        public abstract int Update<TEntity>(TEntity entity);

        public abstract int Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate);

        public abstract int Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> key);

        public abstract int Delete<TEntity>(Expression<Func<TEntity, bool>> predicate);

        public abstract int Delete<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate);

        public abstract int Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> key);

        public abstract int Delete<TEntity>(TEntity entity);

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
