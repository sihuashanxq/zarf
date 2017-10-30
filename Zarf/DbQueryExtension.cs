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

        public static void AddRange<TEntity>(this IDbQuery<TEntity> dbQuery, IEnumerable<TEntity> entities, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.Context?.AddRange(entities, predicate);
        }

        public static void Add<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.Context?.Add(entity);
        }

        public static int Add<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            return dbQuery.Context?.Add(entity, predicate) ?? 0;
        }

        public static int Update<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        {
            return dbQuery.Context?.Update(entity) ?? 0;
        }

        public static int Update<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            return dbQuery.Context?.Update(entity, predicate) ?? 0;
        }

        public static int Update<TEntity, TProperty>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, TProperty>> key)
        {
            return dbQuery.Context?.Update(entity, key) ?? 0;
        }

        public static int Delete<TEntity>(this IDbQuery<TEntity> dbQuery, Expression<Func<TEntity, bool>> predicate)
        {
            return dbQuery.Context?.Delete(predicate) ?? 0;
        }

        public static int Delete<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            return dbQuery.Context?.Delete(entity, predicate) ?? 0;
        }

        public static int Delete<TEntity, TProperty>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, TProperty>> key)
        {
            return dbQuery.Context?.Delete(entity) ?? 0;
        }

        public static int Delete<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        {
            return dbQuery.Context?.Delete(entity) ?? 0;
        }
    }
}
