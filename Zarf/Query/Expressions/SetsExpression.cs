using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Zarf.Queries.Expressions
{
    /// <summary>
    /// 集合表达式
    /// </summary>
    public class SetsExpression : Expression
    {
        public QueryExpression Query { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Query.Type;

        public SetsExpression(QueryExpression query)
        {
            Query = query;
        }

        public override int GetHashCode()
        {
            return Query.GetHashCode();
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

            return GetHashCode() == (obj as SetsExpression)?.GetHashCode();
        }

        public static bool operator ==(SetsExpression left, SetsExpression right)
        {
            if (ReferenceEquals(null, left))
            {
                return ReferenceEquals(null, right);
            }

            return Equals(left, right);
        }


        public static bool operator !=(SetsExpression left, SetsExpression right)
        {
            return !(left == right);
        }
    }
}
