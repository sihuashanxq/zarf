using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class MethodNodeHandler : QueryNodeHandler<MethodCallExpression>
    {
        public MethodNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            return HandleNode(
                Compile<SelectExpression>(methodCall.Arguments[0]),
                methodCall.Arguments.Count == 1 ? null : methodCall.Arguments[1],
                methodCall.Method);
        }
    }
}
