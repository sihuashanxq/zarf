using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Zarf.Entities;
using System.Reflection;
using Zarf.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarf.Query.Expressions
{
    public class FromTableExpression : Expression
    {
        public Table Table { get; protected set; }

        public string Alias { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public QueryExpression Parent { get; protected set; }

        public FromTableExpression(Type type, string alias = "")
        {
            Type = type;
            Alias = alias;

            var attribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (attribute == null)
            {
                Table = new Table(type.Name);
            }
            else
            {
                Table = new Table(attribute.Name, attribute.Schema.IsNullOrEmpty() ? "dbo" : attribute.Schema);
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Table.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                hashCode = (hashCode * 397) ^ (Alias?.GetHashCode() ?? 0);

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

            return (obj is FromTableExpression) && GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(FromTableExpression left, FromTableExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(FromTableExpression left, FromTableExpression right)
        {
            return !(left == right);
        }
    }
}
