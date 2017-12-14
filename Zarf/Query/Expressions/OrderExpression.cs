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

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;
                foreach (var column in Columns)
                {
                    hashCode = (hashCode * 37) ^ column.GetHashCode();
                }

                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return GetHashCode() == (obj as OrderExpression)?.GetHashCode();
        }

        public static bool operator ==(OrderExpression left, OrderExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(OrderExpression left, OrderExpression right)
        {
            return !(left == right);
        }
    }
}