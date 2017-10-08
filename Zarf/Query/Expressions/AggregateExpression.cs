using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 聚合表达式   COUNT SUM  AVG etc..
    /// </summary>
    public class AggregateExpression : Expression
    {
        public MethodInfo Method { get; }

        public Expression KeySelector { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Method.ReturnType;

        public AggregateExpression(MethodInfo method, Expression keySelector)
        {
            Method = method;
            KeySelector = keySelector;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Method.GetHashCode();
                hashCode = (hashCode * 397) ^ (KeySelector?.GetHashCode() ?? 0);
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

            return (obj is AggregateExpression) && GetHashCode() == obj.GetHashCode();
        }
    }
}
