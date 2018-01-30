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

        public SelectExpression Select { get; }

        public string Alias { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Method.ReturnType;

        public AggregateExpression(MethodInfo method, Expression keySelector, SelectExpression select, string alias = "")
        {
            Method = method;
            KeySelector = keySelector;
            Select = select;
            Alias = alias;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Method.GetHashCode();
                hashCode = (hashCode * 37) ^ (KeySelector?.GetHashCode() ?? 0);
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

            return GetHashCode() == (obj as AggregateExpression)?.GetHashCode();
        }

        public static bool operator ==(AggregateExpression left, AggregateExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(AggregateExpression left, AggregateExpression right)
        {
            return !(left == right);
        }
    }
}
