using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 集合表达式
    /// </summary>
    public class SetsExpression : Expression
    {
        public SelectExpression Select { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Select.Type;

        public SetsExpression(SelectExpression select)
        {
            Select = select;
        }

        public override int GetHashCode()
        {
            return Select.GetHashCode();
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
