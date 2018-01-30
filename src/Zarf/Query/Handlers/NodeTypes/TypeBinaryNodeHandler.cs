using System.Linq.Expressions;

namespace Zarf.Query.Handlers.NodeTypes
{
    public class TypeBinaryNodeHandler : QueryNodeHandler<TypeBinaryExpression>
    {
        public TypeBinaryNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) 
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(TypeBinaryExpression typeBinary)
        {
            var expType = typeBinary.Expression.Type;
            return typeBinary.TypeOperand.IsAssignableFrom(typeBinary.Expression.Type)
                ? Utils.ExpressionTrue
                : Utils.ExpressionFalse;
        }
    }
}
