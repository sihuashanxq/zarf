using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Query.ExpressionTranslators
{
    public abstract class Translator<TExpression> : ITranslator<TExpression>, ITranslaor
    {
        public abstract Expression Translate(QueryContext context, TExpression expression, ExpressionVisitor tranformVisitor);

        public Expression Translate(QueryContext context, Expression expression, ExpressionVisitor tranformVisitor)
        {
            return Translate(context, expression.Cast<TExpression>(), tranformVisitor);
        }
    }
}
