using System.Linq.Expressions;

namespace Zarf.Query.Handlers
{
    public interface IQueryNodeHandlerProvider
    {
        IQueryNodeHandler GetHandler(IQueryContext queryContext, IQueryCompiler queryCompiler, Expression node);
    }
}
