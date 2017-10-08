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

            return (obj is Projection) && GetHashCode() == obj.GetHashCode();
        }
    }
}
