using Zarf.Extensions;
using System.Linq.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public abstract class ExpressionVisitorBase : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            switch (node.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(node.Cast<LambdaExpression>());
                default:
                    return base.Visit(node);
            }
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            return lambda;
        }
    }
}