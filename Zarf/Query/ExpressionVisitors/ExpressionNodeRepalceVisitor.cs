using System.Linq.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class InnerNodeUpdateExpressionVisitor : ExpressionVisitorBase
    {
        protected Expression BeUpdatedNode { get; set; }

        protected Expression UpdateValueNode { get; set; }

        public InnerNodeUpdateExpressionVisitor(Expression beUpdaedNode, Expression updateValueNode)
        {
            BeUpdatedNode = beUpdaedNode;
            UpdateValueNode = updateValueNode;
        }

        public override Expression Visit(Expression node)
        {
            if (BeUpdatedNode == node)
            {
                return UpdateValueNode;
            }

            return base.Visit(node);
        }

        public Expression Update(Expression node, Expression beUpdaedNode = null, Expression updateValueNode = null)
        {
            lock (this)
            {
                if (beUpdaedNode != null)
                {
                    BeUpdatedNode = beUpdaedNode;
                }

                if (updateValueNode != null)
                {
                    UpdateValueNode = updateValueNode;
                }

                if (BeUpdatedNode == null ||
                    UpdateValueNode == null ||
                    BeUpdatedNode == UpdateValueNode)
                {
                    return node;
                }

                return Visit(node);
            }
        }
    }
}
