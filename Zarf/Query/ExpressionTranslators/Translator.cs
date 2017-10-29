using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Query.ExpressionTranslators
{
    public abstract class Translator<TExpression> : ITranslator<TExpression>, ITranslaor
    {
        public abstract Expression Translate(IQueryContext context, TExpression query, IQueryCompiler queryCompiler);

        public Expression Translate(IQueryContext context, Expression query, IQueryCompiler queryCompiler)
        {
            return Translate(context, query.Cast<TExpression>(), queryCompiler);
        }
    }
}
