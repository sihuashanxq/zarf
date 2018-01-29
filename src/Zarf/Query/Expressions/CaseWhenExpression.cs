using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class CaseWhenExpression : Expression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        public Expression CaseWhen { get; }

        public Expression Then { get; }

        public Expression Else { get; }

        public CaseWhenExpression(Expression caseWhen, Expression then, Expression @else, Type type)
        {
            CaseWhen = caseWhen;
            Then = then;
            Else = @else;
            Type = type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type?.GetHashCode() ?? 0;
                hashCode += (hashCode * 37) ^ (CaseWhen?.GetHashCode() ?? 0);
                hashCode += (hashCode * 37) ^ (Then?.GetHashCode() ?? 0);
                hashCode += (hashCode * 37) ^ (Else?.GetHashCode() ?? 0);
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

            return GetHashCode() == (other as CaseWhenExpression)?.GetHashCode();
        }

        public static bool operator ==(CaseWhenExpression left, CaseWhenExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(CaseWhenExpression left, CaseWhenExpression right)
        {
            return !(left == right);
        }
    }
}
