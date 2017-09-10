using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Zarf.Query
{
    /// <summary>
    /// 查找引用的投影接口
    /// </summary>
    public interface IProjectionScanner
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TRefrence">is ExpressionType.Extension</typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        List<TRefrence> Scan<TRefrence>(Expression node)
            where TRefrence : Expression;

        /// <summary>
        /// </summary>
        /// <typeparam name="TRefrence">is ExpressionType.Extension</typeparam>
        /// <param name="preHandle"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        List<TRefrence> Scan<TRefrence>(Func<Expression, Expression> preHandle, Expression node)
            where TRefrence : Expression;

        List<Expression> Scan(Expression node);

        List<Expression> Scan(Func<Expression, Expression> preHandle, Expression node);
    }
}
