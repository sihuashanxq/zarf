using System;
using System.Linq.Expressions;

namespace Zarf.Queries.Expressions
{
    public class AliasExpression : SourceExpression
    {
        public string Alias { get; internal set; }

        public Expression Expression { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _type ?? Expression?.Type;

        private Type _type;

        public AliasExpression(string alias, Expression exp, Expression source, Type type = null)
            : base(source)
        {
            _type = type;
            Alias = alias;
            Expression = exp;
        }
    }
}
