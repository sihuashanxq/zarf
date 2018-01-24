using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public interface ITransaltorProvider
    {
        ITranslator GetTranslator(IQueryContext queryContext, IQueryCompiler queryCompiler, Expression node);
    }
}
