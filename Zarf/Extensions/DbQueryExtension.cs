using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;

namespace Zarf
{
    public static class DbQueryExtension
    {
        //public static void AddRange<TEntity>(this IDbQuery<TEntity> dbQuery, IEnumerable<TEntity> entities)
        //{
        //    dbQuery.Context?.AddRange(entities);
        //}

        //public static void Add<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        //{
        //    dbQuery.Context?.Add(entity);
        //}

        //public static void Update<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        //{
        //    dbQuery.Context?.Update(entity);
        //}

        //public static void Update<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        //{
        //    dbQuery.Context?.Update(entity, predicate);
        //}

        //public static void Delete<TEntity>(this IDbQuery<TEntity> dbQuery, Expression<Func<TEntity, bool>> predicate)
        //{
        //    dbQuery.Context?.Delete(predicate);
        //}

        //public static void Delete<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        //{
        //    dbQuery.Context?.Delete(entity, predicate);
        //}

        //public static void Delete<TEntity>(this IDbQuery<TEntity> dbQuery, TEntity entity)
        //{
        //    dbQuery.Context?.Delete(entity);
        //}

        /// <summary>
        /// 查询包含属性
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="dbQuery">原始查询</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="propertyRelation">关联关系</param>
        /// <returns></returns>
        internal static IInternalDbQuery<TEntity> Include<TEntity, TProperty>(
             this IInternalDbQuery<TEntity> dbQuery,
             Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath,
             Expression<Func<TEntity, TProperty, bool>> propertyRelation
           )
        {
            return new InternalDbQuery<TEntity>(dbQuery.Provider,
                  Expression.Call(
                      ReflectionUtil.Include.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
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
        internal static IInternalDbQuery<TEntity> ThenInclude<TEntity, TPrevious, TProperty>(
            this IInternalDbQuery<TEntity> dbQuery,
            Expression<Func<TPrevious, IEnumerable<TProperty>>> propertyPath,
            Expression<Func<TPrevious, TProperty, bool>> propertyRelation
            )
        {
            return new InternalDbQuery<TEntity>(dbQuery.Provider,
                     Expression.Call(
                     ReflectionUtil.ThenInclude.MakeGenericMethod(typeof(TEntity), typeof(TPrevious), typeof(TProperty)),
                     dbQuery.Expression,
                     Expression.Quote(propertyPath),
                     Expression.Quote(propertyRelation)
                 ));
        }
    }
}
