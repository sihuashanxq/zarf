using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public interface ITranslator<in T> : ITranslator
    {
        Expression Translate(T query);
    }

    public interface ITranslator
    {
        IQueryCompiler QueryCompiler { get; }

        IQueryContext QueryContext { get; }

        Expression Translate(Expression query);
    }
}
