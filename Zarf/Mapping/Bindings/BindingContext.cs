using System.Linq.Expressions;
using Zarf.Queries;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Expression Query { get; }

        public IQueryExecutor QueryExecutor { get; }

        public BindingContext(Expression query, IQueryExecutor executor)
        {
            Query = query;
            QueryExecutor = executor;
        }
    }
}
