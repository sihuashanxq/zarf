using System.Linq.Expressions;
using System.Collections.Generic;

namespace Zarf.Queries
{
    public interface IQueryExecutor
    {
        IEnumerator<TEntity> Execute<TEntity>(Expression query, IQueryContext queryContext);

        TEntity ExecuteSingle<TEntity>(Expression query, IQueryContext queryContext);
    }
}
