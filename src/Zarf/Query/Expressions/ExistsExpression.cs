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
            Select = select;
        }
    }
}
