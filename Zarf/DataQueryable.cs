using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace Zarf
{
    public static class DataQueryable
    {
        public static readonly MethodInfo IncludeMethodInfo;

        public static readonly MethodInfo ThenIncludeMethodInfo;

        static DataQueryable()
        {
            IncludeMethodInfo = typeof(DataQueryable).GetMethod(nameof(Include));
            ThenIncludeMethodInfo = typeof(DataQueryable).GetMethod(nameof(ThenInclude));
        }

        /// <summary>
        /// 查询包含属性
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="source">原始查询</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="propertyRelation">关联关系</param>
        /// <returns></returns>
        public static IIncludeDataQuery<TEntity, TProperty> Include<TEntity, TProperty>(
             this IDbQuery<TEntity> source,
             Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath,
             Expression<Func<TEntity, TProperty, bool>> propertyRelation
           )
        {
            return new IncludeDataQuery<TEntity, TProperty>(
                new DbQueryProvider(),
                Expression.Call(
                    IncludeMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
                    source.Expression,
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
        /// <param name="source">原始查询</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="propertyRelation">关联关系</param>
        /// <returns></returns>
        public static IIncludeDataQuery<TEntity, TProperty> ThenInclude<TEntity, TPrevious, TProperty>(
           this IIncludeDataQuery<TEntity, TPrevious> source,
            Expression<Func<TPrevious, IEnumerable<TProperty>>> propertyPath,
            Expression<Func<TPrevious, TProperty, bool>> propertyRelation
            )
        {
            return new IncludeDataQuery<TEntity, TProperty>(
                new DbQueryProvider(),
                Expression.Call(
                  ThenIncludeMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TPrevious), typeof(TProperty)),
                  source.Expression,
                  Expression.Quote(propertyPath),
                  Expression.Quote(propertyRelation)
            ));
        }
    }
}
