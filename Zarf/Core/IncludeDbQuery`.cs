using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf
{
    public class IncludeDbQuery<TEntity, TProperty> : DbQuery<TEntity>, IIncludeDbQuery<TEntity, TProperty>
    {
        public IncludeDbQuery(IInternalDbQuery<TEntity> internalDbQuery) : base(internalDbQuery)
        {

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
        public IIncludeDbQuery<TEntity, TKey> ThenInclude<TKey>(
            Expression<Func<TProperty, IEnumerable<TKey>>>
            propertyPath, Expression<Func<TProperty, TKey, bool>> propertyRelation)
        {
            return new IncludeDbQuery<TEntity, TKey>(
                InternalDbQuery.ThenInclude(
                    propertyPath,
                    propertyRelation ?? CreateDeafultKeyRealtion<TProperty, TKey>()
                )
            );
        }

        public IIncludeDbQuery<TEntity, TKey> ThenInclude<TKey>(Expression<Func<TProperty, IEnumerable<TKey>>> propertyPath)
        {
            return ThenInclude(propertyPath, null);
        }
    }
}
