using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class ExistsExpression : Expression
    {
        public SelectExpression Select { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(bool);

        public ExistsExpression(SelectExpression select)
        {
            select.Projections.Clear();
            select.Groups.Clear();
            select.Orders.Clear();
            select.AddProjection(Expression.Constant(1));
            select.Limit = 1;
            Select = select;
        }
    }
}
