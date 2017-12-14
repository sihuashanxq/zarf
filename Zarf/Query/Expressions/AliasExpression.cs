using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class AliasExpression : SourceExpression
    {
        public string Alias { get; internal set; }

        public Expression Expression { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Expression?.Type;

        public AliasExpression(string alias, Expression exp, Expression source)
            : base(source)
        {
            Alias = alias;
            Expression = exp;
        }
    }
}
