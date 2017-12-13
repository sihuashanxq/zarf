using System;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf.Query.Expressions
{
    public class ColumnExpression : Expression
    {
        private Type _type;

        public Column Column { get; set; }

        public MemberInfo Member { get; set; }

        public Expression Source { get; set; }

        public QueryExpression Query { get; set; }

        public string Alias { get; set; }

        public override Type Type => Member?.GetPropertyType() ?? _type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public ColumnExpression(QueryExpression query, MemberInfo member, string alias = "")
        {
            Query = query;
            Member = member;
            if (Member != null)
            {
                Column = Member.ToColumn();
                Alias = alias;
            }
        }

        public ColumnExpression(QueryExpression query, Column column, Type valueType, string alias = "")
        {
            Query = query;
            Column = column;
            _type = valueType;
        }

        public ColumnExpression Clone()
        {
            return new ColumnExpression(Query, Member, Alias)
            {
                _type = _type,
                Column = Column
            };
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Query?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Member?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Type?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Column?.Name.GetHashCode() ?? 0);
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

            return GetHashCode() == (other as ColumnExpression)?.GetHashCode();
        }

        public static bool operator ==(ColumnExpression left, ColumnExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(ColumnExpression left, ColumnExpression right)
        {
            return !(left == right);
        }
    }
}
