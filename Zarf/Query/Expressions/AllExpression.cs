using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class AllExpression : Expression
    {
        public Expression Expression { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        public AllExpression(Expression expression)
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

            return GetHashCode() == (obj as AllExpression)?.GetHashCode();
        }

        public static bool operator ==(AllExpression left, AllExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(AllExpression left, AllExpression right)
        {
            return !(left == right);
        }
    }
}
