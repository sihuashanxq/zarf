using System.Linq.Expressions;

namespace Zarf.Queries.ExpressionTranslators.NodeTypes
{
    public class TypeBinaryExpressionTranslator : Translator<TypeBinaryExpression>
    {
        public TypeBinaryExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) 
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(TypeBinaryExpression typeBinary)
        {
            var expType = typeBinary.Expression.Type;
            return typeBinary.TypeOperand.IsAssignableFrom(typeBinary.Expression.Type)
                ? Utils.ExpressionTrue
                : Utils.ExpressionFalse;
        }
    }
}
