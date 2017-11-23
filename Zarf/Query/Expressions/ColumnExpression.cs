using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarf.Query.Expressions
{
    public class ColumnExpression : AliasExpression
    {
        public Column Column { get; }

        public MemberInfo Member { get; }

        public FromTableExpression FromTable { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public ColumnExpression(FromTableExpression table, MemberInfo member, string alias = "")
            : base(alias)
        {
            FromTable = table;
            Member = member;

            if (Member != null)
            {
                Type = Member.GetMemberTypeInfo();
                Column = Member.ToColumn();
                Alias = alias;
            }
        }

        public ColumnExpression(FromTableExpression table, Column column, Type valueType, string alias = "")
            : base(alias)
        {
            FromTable = table;
            Column = column;
            Type = valueType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FromTable?.GetHashCode() ?? 0;
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
