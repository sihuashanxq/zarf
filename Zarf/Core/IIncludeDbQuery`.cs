using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf
{
    public interface IIncludeDbQuery<TEntity, TProperty> : IDbQuery<TEntity>
    {
        IIncludeDbQuery<TEntity, TKey> ThenInclude<TKey>(Expression<Func<TProperty, IEnumerable<TKey>>> propertyPath, Expression<Func<TProperty, TKey, bool>> propertyRelation);
    }
}
