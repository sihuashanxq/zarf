using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class ParameterExpressionTranslator : Translator<ParameterExpression>
    {
        public ParameterExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(ParameterExpression parameter)
        {
            return Context.QuerySourceProvider.GetSource(parameter) ?? parameter;
        }
    }
}
