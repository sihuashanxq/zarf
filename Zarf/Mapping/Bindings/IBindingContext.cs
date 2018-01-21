using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Expression Expression { get; }

        IQueryExecutor QueryExecutor { get; }
    }
}
