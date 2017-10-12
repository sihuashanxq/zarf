using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class GroupExpression : Expression
    {
        internal static readonly Type ObjectType = typeof(object);

        public IEnumerable<ColumnExpression> Columns { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => ObjectType;

        public GroupExpression(IEnumerable<ColumnExpression> columns)
        {
            Columns = columns;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;
                foreach (var column in Columns)
                {
                    hashCode = (hashCode * 397) ^ column.GetHashCode();
                }

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

            return (obj is GroupExpression) && GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(GroupExpression left, GroupExpression right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(GroupExpression left, GroupExpression right)
        {
            return !(left == right);
        }
    }
}