using System;
using System.Linq;
using System.Linq.Expressions;
namespace Zarf.Core.Internals
{
    public interface IInternalQuery
    {
        Expression GetExpression();

        Type GetTypeOfEntity();
    }

    public interface IInternalQuery<TEntity> : IInternalQuery, IQueryable<TEntity>, IOrderedQueryable<TEntity>
    {

    }
}
