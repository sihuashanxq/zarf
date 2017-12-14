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

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Offset.GetHashCode();
                foreach (var order in Orders)
                {
                    hashCode = (hashCode * 37) ^ order.GetHashCode();
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

            return GetHashCode() == (obj as SkipExpression)?.GetHashCode();
        }

        public static bool operator ==(SkipExpression left, SkipExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(SkipExpression left, SkipExpression right)
        {
            return !(left == right);
        }
    }
}
