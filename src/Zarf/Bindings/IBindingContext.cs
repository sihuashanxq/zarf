using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Bindings
{
    public interface IBindingContext
    {
        Expression SourceExpression { get; }

        Expression ModelExpression { get; }

        IQueryExecutor QueryExecutor { get; }

        IQueryContext QueryContext { get; }

        bool CacheModelElementCreator { get; set; }
    }
}
