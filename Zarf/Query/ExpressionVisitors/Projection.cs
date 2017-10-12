using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class Projection
    {
        public Expression Expression { get; set; }

        public int Ordinal { get; set; } = -1;

        public MemberInfo Member { get; set; }

        public FromTableExpression Query { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Query.GetHashCode();
                hashCode = (hashCode * 397) ^ (Expression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Member?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (other is Projection) && GetHashCode() == other.GetHashCode();
        }


        public static bool operator ==(Projection left, Projection right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Projection left, Projection right)
        {
            return !(left == right);
        }
    }
}
