using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 聚合表达式   COUNT SUM  AVG etc..
    /// </summary>
    public class AggregateExpression : Expression
    {
        public MethodInfo Method { get; }

        public Expression KeySelector { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Method.ReturnType;

        public AggregateExpression(MethodInfo method, Expression keySelector)
        {
            Method = method;
            KeySelector = keySelector;
        }
    }
}
