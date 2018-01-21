using System.Linq.Expressions;
using Zarf.Query;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class MethodTranslator : Translator<MethodCallExpression>
    {
        public MethodTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            return Translate(
                Compile<SelectExpression>(methodCall.Arguments[0]),
                methodCall.Arguments.Count == 1 ? null : methodCall.Arguments[1],
                methodCall.Method);
        }
    }
}
