using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Bindings
{
    public interface IBindingContext
    {
        Expression Expression { get; }

        IQueryExecutor QueryExecutor { get; }
    }
}
