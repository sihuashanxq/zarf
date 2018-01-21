using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Expression Expression { get; }

        public IQueryExecutor QueryExecutor { get; }

        public BindingContext(Expression query, IQueryExecutor executor)
        {
            Expression = query;
            QueryExecutor = executor;
        }
    }
}
