using System.Linq.Expressions;
using System;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    ///<summary>
    ///Represent a join query 
    ///</summary>
    public class JoinExpression : Expression
    {
        public JoinType JoinType { get; }

        public Expression Predicate { get; }

        public QueryExpression Query { get; }

        public override Type Type => Query?.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public JoinExpression(QueryExpression query, Expression predicate, JoinType joinType = JoinType.Inner)
        {
            Predicate = predicate;
            Query = query;
            JoinType = joinType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Predicate.GetHashCode();
                hashCode += (hashCode * 37) ^ (Query?.GetHashCode() ?? 0);
                hashCode += (hashCode * 37) ^ JoinType.GetHashCode();
                hashCode += (hashCode * 37) ^ (Predicate?.GetHashCode() ?? 0);
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

            return GetHashCode() == (obj as JoinExpression).GetHashCode();
        }

        public static bool operator ==(JoinExpression left, JoinExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(JoinExpression left, JoinExpression right)
        {
            return !(left == right);
        }
    }
}