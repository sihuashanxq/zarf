using System;
using System.Linq.Expressions;
using Zarf.Extensions;
namespace Zarf.Query.Expressions
{
    public class WhereExperssion : Expression
    {
        public Expression Predicate { get; private set; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Predicate.Type;

        public WhereExperssion(Expression condition)
        {
            if (condition.NodeType == ExpressionType.Lambda)
            {
                condition = (condition as LambdaExpression).Body;
            }

            if (condition.Is<ConstantExpression>())
            {
                condition = (bool)condition.As<ConstantExpression>().Value
                    ? Utils.ExpressionTrue
                    : Utils.ExpressionFalse;
            }

            Predicate = condition;
        }

        public void Combine(Expression condition)
        {
            if (condition.NodeType == ExpressionType.Lambda)
            {
                condition = (condition as LambdaExpression).Body;
            }

            Predicate = AndAlso(Predicate, condition);
        }

        public override int GetHashCode()
        {
            return Predicate.GetHashCode();
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

            return (obj is WhereExperssion) && GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(WhereExperssion left, WhereExperssion right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(WhereExperssion left, WhereExperssion right)
        {
            return !(left == right);
        }
    }
}
