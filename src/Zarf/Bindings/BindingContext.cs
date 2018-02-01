using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Expression ModelExpression { get; }

        public Expression SourceExpression { get; }

        public IQueryExecutor QueryExecutor { get; }

        public IQueryContext QueryContext { get; }

        public bool CacheModelElementCreator { get; set; }

        public BindingContext(
            Expression modelExpression,
            Expression sourceExpression,
            IQueryExecutor queryExecutor,
            IQueryContext queryContext)
        {
            QueryContext = queryContext;
            QueryExecutor = queryExecutor;
            ModelExpression = modelExpression;
            SourceExpression = sourceExpression;
            CacheModelElementCreator = true;
        }
    }
}
