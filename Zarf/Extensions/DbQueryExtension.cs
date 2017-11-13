using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf
{
    public static class DbQueryExtension
    {
        public static void AddRange<TEntity>(this IDbQuery<TEntity> dbQuery, IEnumerable<TEntity> entities)
        {
            dbQuery.Context?.AddRange(entities);
        }

        public static void Add<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.Context?.Add(entity);
        }

        public static void Update<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.Context?.Update(entity);
        }

        public static void Update<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.Context?.Update(entity, predicate);
        }

        public static void Delete<TEntity>(this IDbQuery<TEntity> dbQuery, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.Context?.Delete(predicate);
        }

        public static void Delete<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.Context?.Delete(entity, predicate);
        }

        public static void Delete<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.Context?.Delete(entity);
        }
    }
}
