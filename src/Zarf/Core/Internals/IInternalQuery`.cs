using System;
using System.Linq;
using System.Linq.Expressions;
namespace Zarf.Core.Internals
{
    public interface IInternalQuery
    {
        Expression Expression { get; }

        Type ElementType { get; }
    }

    public interface IInternalQuery<TEntity> : IInternalQuery, IQueryable<TEntity>, IOrderedQueryable<TEntity>
    {
        new Expression Expression { get; }

        new Type ElementType { get; }
    }
}
