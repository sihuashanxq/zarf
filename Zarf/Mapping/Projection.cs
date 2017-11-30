using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping
{
    public class ColumnDescriptor
    {
        public ColumnDescriptor()
        {

        }

        public ColumnDescriptor(Expression exp)
        {
            Expression = exp;
        }

        public Expression Expression { get; set; }

        public int Ordinal { get; set; } = -1;

        public MemberInfo Member { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;
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

            return (other is ColumnDescriptor) && GetHashCode() == other.GetHashCode();
        }


        public static bool operator ==(ColumnDescriptor left, ColumnDescriptor right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(ColumnDescriptor left, ColumnDescriptor right)
        {
            return !(left == right);
        }
    }
}
