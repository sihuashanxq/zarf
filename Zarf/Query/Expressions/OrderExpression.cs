using System.Linq.Expressions;
using System.Collections.Generic;
using System;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    public class OrderExpression : Expression
    {
        public IEnumerable<ColumnExpression> Columns { get; }

        public OrderType OrderType { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(object);

        public OrderExpression(IEnumerable<ColumnExpression> columns, OrderType orderType = OrderType.Asc)
        {
            Columns = columns;
            OrderType = orderType;
        }
    }
}