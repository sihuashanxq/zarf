using System;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Metadata.Entities;

namespace Zarf.Query.Expressions
{
    public class ColumnExpression : Expression
    {
        private Type _type;

        public Column Column { get; set; }

        public MemberInfo Member { get; set; }

        public SelectExpression Select { get; set; }

        public string Alias { get; set; }

        public override Type Type => Member?.GetPropertyType() ?? _type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public ColumnExpression(SelectExpression select, MemberInfo member, string alias = "")
        {
            Select = select;
            Member = member;
            if (Member != null)
            {
                Column = Member.ToColumn();
                Alias = alias;
            }
        }

        public ColumnExpression(SelectExpression select, Column column, Type valueType, string alias = "")
        {
            Select = select;
            Column = column;
            _type = valueType;
        }

        public ColumnExpression Clone(string alias = "")
        {
            return new ColumnExpression(Select, Member, Alias)
            {
                _type = _type,
                Alias = alias,
                Column = Column
            };
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Select?.GetHashCode() ?? 0;
                hashCode += (hashCode * 37) ^ (Alias?.GetHashCode() ?? 0);
                hashCode += (hashCode * 37) ^ (Member?.GetHashCode() ?? 0);
                hashCode += (hashCode * 37) ^ (Type?.GetHashCode() ?? 0);
                hashCode += (hashCode * 37) ^ (Column?.Name.GetHashCode() ?? 0);
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
