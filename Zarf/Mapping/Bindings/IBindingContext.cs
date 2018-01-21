using System.Linq.Expressions;
using Zarf.Queries;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Expression Query { get; }

        IQueryExecutor QueryExecutor { get; }
    }
}
