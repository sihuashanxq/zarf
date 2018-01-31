using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Infrastructure
{
    /// <summary>
    /// 表达式相等比较
    /// 先用HashCode顶一下
    /// </summary>
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        static ExprssionEqualityHashCodeCalculator CodeCalculator;

        static ExpressionEqualityComparer()
        {
            CodeCalculator = new ExprssionEqualityHashCodeCalculator();
        }

        public virtual int GetHashCode(Expression obj)
        {
            return CodeCalculator.GetHashCode(obj);
        }

        public virtual bool Equals(Expression x, Expression y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }
    }
}
