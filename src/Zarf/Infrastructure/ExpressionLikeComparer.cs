using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Infrastructure
{
    /// <summary>
    /// 表达式相识比较
    /// </summary>
    public class ExpressionLikeComparer : IEqualityComparer<Expression>
    {
        static ExprssionLikeHashCodeCalculator CodeCalculator;

        static ExpressionLikeComparer()
        {
            CodeCalculator = new ExprssionLikeHashCodeCalculator();
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
