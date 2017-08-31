using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class ParameterExpressionTranslator : Translator<ParameterExpression>
    {
        public override Expression Translate(QueryContext context, ParameterExpression parameter, ExpressionVisitor transformVisitor)
        {
            if (context.QuerySource.TryGetValue(parameter, out QueryExpression query))
            {
                return query;
            }

            return parameter;
        }
    }
}
