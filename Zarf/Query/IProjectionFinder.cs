using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Zarf.Queries.ExpressionVisitors;
using Zarf.Mapping;

namespace Zarf.Queries
{
    /// <summary>
    /// 查找引用的投影接口
    /// </summary>
    public interface IProjectionScanner
    {
        List<ColumnDescriptor> Scan(Func<Expression, Expression> preHandle, Expression node);

        List<ColumnDescriptor> Scan(Expression node);
    }
}
