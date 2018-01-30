using System.Linq.Expressions;

namespace Zarf.Query.Handlers.NodeTypes
{
    public class ParameterNodeHandler : QueryNodeHandler<ParameterExpression>
    {
        public ParameterNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(ParameterExpression parameter)
        {
            return QueryContext.SelectMapper.GetValue(parameter) as Expression ?? parameter;
        }
    }
}
