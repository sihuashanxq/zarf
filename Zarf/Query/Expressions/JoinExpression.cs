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

        public FromTableExpression Table { get; }

        public override Type Type => Table?.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public JoinExpression(FromTableExpression table, Expression predicate, JoinType joinType = JoinType.Inner)
        {
            Predicate = predicate;
            Table = table;
            JoinType = joinType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Predicate.GetHashCode();
                hashCode = (hashCode * 397) ^ (Table?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ JoinType.GetHashCode();
                hashCode = (hashCode * 397) ^ (Predicate?.GetHashCode() ?? 0);
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