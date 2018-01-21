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

        public WhereExperssion(Expression condtion)
        {
            Combine(condtion);
        }

        public void Combine(Expression condtion)
        {
            var predicate = condtion.UnWrap().As<LambdaExpression>()?.Body ?? condtion;
            if (predicate.Is<ConstantExpression>())
            {
                predicate = (bool)predicate.As<ConstantExpression>().Value
                    ? Utils.ExpressionTrue
                    : Utils.ExpressionFalse;
            }

            if (Predicate == null)
            {
                Predicate = predicate;
            }
            else
            {
                Predicate = AndAlso(Predicate, predicate);
            }
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

            return GetHashCode() == (obj as WhereExperssion)?.GetHashCode();
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
