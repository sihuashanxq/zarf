using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query
{
    /// <summary>
    /// 查找引用的投影接口
    /// </summary>
    public interface IProjectionScanner
    {
        List<Projection> Scan(Func<Expression, Expression> preHandle, Expression node);

        List<Projection> Scan(Expression node);
    }
}
