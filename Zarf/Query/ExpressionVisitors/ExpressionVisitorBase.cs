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
                    node = VisitLambda(node.Cast<LambdaExpression>());
                    break;
                default:
                    node = base.Visit(node);
                    break;
            }

            return node;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            return lambda;
        }
    }
}