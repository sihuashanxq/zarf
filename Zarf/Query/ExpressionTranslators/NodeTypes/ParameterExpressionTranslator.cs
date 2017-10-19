using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class ParameterExpressionTranslator : Translator<ParameterExpression>
    {
        public override Expression Translate(IQueryContext context, ParameterExpression parameter, ExpressionVisitor transformVisitor)
        {
            return context.QuerySourceProvider.GetSource(parameter) ?? parameter;
        }
    }
}
