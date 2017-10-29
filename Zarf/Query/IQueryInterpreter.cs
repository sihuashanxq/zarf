using System.Linq.Expressions;
using System.Collections.Generic;

namespace Zarf.Query
{
    public interface IQueryInterpreter
    {
        IEnumerator<TEntity> Execute<TEntity>(Expression query, IQueryContext queryContext = null);

        TEntity ExecuteSingle<TEntity>(Expression query, IQueryContext queryContext = null);
    }
}
