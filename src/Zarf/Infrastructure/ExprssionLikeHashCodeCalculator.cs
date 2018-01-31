using System.Linq.Expressions;

namespace Zarf.Infrastructure
{
    /// <summary>
    /// 表达式相识HashCode计算
    /// </summary>
    public class ExprssionLikeHashCodeCalculator : ExprssionEqualityHashCodeCalculator
    {
        /// <summary>
        /// 获取常数HashCode
        /// </summary>
        /// <param name="constant"></param>
        /// <returns></returns>
        protected override int GetHashCode(ConstantExpression constant)
        {
            //只计算常数类型HashCode
            return constant.Type?.GetHashCode() ?? 0;
        }
    }
}
