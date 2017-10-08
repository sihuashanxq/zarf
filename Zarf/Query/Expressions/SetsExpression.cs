using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 集合表达式
    /// </summary>
    public class SetsExpression : Expression
    {
        public QueryExpression Query { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Query.Type;

        public SetsExpression(QueryExpression query)
        {
            Query = query;
        }


    }
}
