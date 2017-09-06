using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class ParameterExpressionTranslator : Translator<ParameterExpression>
    {
        public override Expression Translate(IQueryContext context, ParameterExpression parameter, ExpressionVisitor transformVisitor)
        {
            var refrenceQuery = context.QuerySourceProvider.GetSource(parameter);

            if (refrenceQuery == null)
            {
                return parameter;
            }

            return refrenceQuery;
        }
    }
}
