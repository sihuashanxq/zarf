using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Zarf.Query
{
    public interface IRefrenceProjectionFinder
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TRefrence">is ExpressionType.Extension</typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        List<TRefrence> Find<TRefrence>(Expression node)
            where TRefrence : Expression;

        /// <summary>
        /// </summary>
        /// <typeparam name="TRefrence">is ExpressionType.Extension</typeparam>
        /// <param name="preHandle"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        List<TRefrence> Find<TRefrence>(Func<Expression, Expression> preHandle, Expression node)
            where TRefrence : Expression;

        List<Expression> Find(Expression node);

        List<Expression> Find(Func<Expression, Expression> preHandle, Expression node);
    }
}
