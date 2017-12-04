using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf
{
    public interface IIncludeQuery<TEntity, TProperty> : IQuery<TEntity>
    {
        IIncludeQuery<TEntity, TKey> ThenInclude<TKey>(Expression<Func<TProperty, IEnumerable<TKey>>> propertyPath, Expression<Func<TProperty, TKey, bool>> propertyRelation );

        IIncludeQuery<TEntity, TKey> ThenInclude<TKey>(Expression<Func<TProperty, IEnumerable<TKey>>> propertyPath);
    }
}
