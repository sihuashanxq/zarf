using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Core.Internals;

namespace Zarf
{
    public class IncludeQuery<TEntity, TProperty> : Query<TEntity>, IIncludeQuery<TEntity, TProperty>
    {
        public IncludeQuery(IInternalQuery<TEntity> internalDbQuery) : base(internalDbQuery)
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
        public IIncludeQuery<TEntity, TKey> ThenInclude<TKey>(
            Expression<Func<TProperty, IEnumerable<TKey>>>
            propertyPath, Expression<Func<TProperty, TKey, bool>> propertyRelation)
        {
            return new IncludeQuery<TEntity, TKey>(
                InternalQuery.ThenInclude(
                    propertyPath,
                    propertyRelation ?? CreateDeafultKeyRealtion<TProperty, TKey>()
                )
            );
        }

        public IIncludeQuery<TEntity, TKey> ThenInclude<TKey>(Expression<Func<TProperty, IEnumerable<TKey>>> propertyPath)
        {
            return ThenInclude(propertyPath, null);
        }
    }
}
