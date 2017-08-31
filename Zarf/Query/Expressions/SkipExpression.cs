using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Zarf.Query.Expressions
{
    public class SkipExpression : Expression
    {
        public int Offset { get; }

        public List<OrderExpression> Orders { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(int);

        public SkipExpression(int offset, List<OrderExpression> orders)
        {
            Offset = offset;
            Orders = orders;
        }
    }
}
