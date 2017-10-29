using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public interface ITranslator<in T> : ITranslaor
    {
        Expression Translate(IQueryContext queryContext, T query, IQueryCompiler queryCompiler);
    }

    public interface ITranslaor
    {
        Expression Translate(IQueryContext queryContext, Expression query, IQueryCompiler queryCompiler);
    }
}
