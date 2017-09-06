using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class TypeBinaryExpressionTranslator : Translator<TypeBinaryExpression>
    {
        public override Expression Translate(IQueryContext context, TypeBinaryExpression typeBinary, ExpressionVisitor transformVisitor)
        {
            var expType = typeBinary.Expression.Type;
            return typeBinary.TypeOperand.IsAssignableFrom(typeBinary.Expression.Type)
                ? Utils.ExpressionTrue
                : Utils.ExpressionFalse;
        }
    }
}
