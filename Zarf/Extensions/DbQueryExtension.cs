using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf
{
    public static class DbQueryExtension
    {
        public static readonly MethodInfo IncludeMethod;

        public static readonly MethodInfo ThenIncludeMethod;

        static DbQueryExtension()
        {
            IncludeMethod = typeof(DbQueryExtension).GetMethod(nameof(Include));
            ThenIncludeMethod = typeof(DbQueryExtension).GetMethod(nameof(ThenInclude));
        }

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

        /// <summary>
        /// 查询包含属性
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="dbQuery">原始查询</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="propertyRelation">关联关系</param>
        /// <returns></returns>
        public static IIncludeDataQuery<TEntity, TProperty> Include<TEntity, TProperty>(
             this IDbQuery<TEntity> dbQuery,
             Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath,
             Expression<Func<TEntity, TProperty, bool>> propertyRelation
           )
        {
            return new IncludeDataQuery<TEntity, TProperty>(
                new DbQueryProvider(dbQuery.Context),
                Expression.Call(
                    IncludeMethod.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
                    dbQuery.Expression,
                    Expression.Quote(propertyPath),
                    Expression.Quote(propertyRelation)
            ));
        }

        /// <summary>
        /// 查询包含属性
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TPrevious">上一个被包含属性类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="dbQuery">原始查询</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="propertyRelation">关联关系</param>
        /// <returns></returns>
        public static IIncludeDataQuery<TEntity, TProperty> ThenInclude<TEntity, TPrevious, TProperty>(
           this IIncludeDataQuery<TEntity, TPrevious> dbQuery,
            Expression<Func<TPrevious, IEnumerable<TProperty>>> propertyPath,
            Expression<Func<TPrevious, TProperty, bool>> propertyRelation
            )
        {
            return new IncludeDataQuery<TEntity, TProperty>(
                new DbQueryProvider(dbQuery.Context),
                Expression.Call(
                  ThenIncludeMethod.MakeGenericMethod(typeof(TEntity), typeof(TPrevious), typeof(TProperty)),
                  dbQuery.Expression,
                  Expression.Quote(propertyPath),
                  Expression.Quote(propertyRelation)
            ));
        }
    }
}
