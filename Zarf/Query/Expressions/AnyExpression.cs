using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class AnyExpression : Expression
    {
        public QueryExpression Query { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        public AnyExpression(QueryExpression query)
        {
            Type = typeof(bool);
            Query = query;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Query.GetHashCode();
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

            return GetHashCode() == (obj as AnyExpression).GetHashCode();
        }

        public static bool operator ==(AnyExpression left, AnyExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(AnyExpression left, AnyExpression right)
        {
            return !(left == right);
        }
    }
}
