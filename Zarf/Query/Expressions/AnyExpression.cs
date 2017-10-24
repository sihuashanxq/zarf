using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class AnyExpression : Expression
    {
        public Expression Expression { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        public AnyExpression(Expression expression)
        {
            Type = typeof(bool);
            Expression = expression;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Expression.GetHashCode();
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
