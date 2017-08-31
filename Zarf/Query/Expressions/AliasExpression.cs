using System;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class AliasExpression : Expression
    {
        public string Alias { get; internal set; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        public AliasExpression(string alias, Type type = null)
        {
            Alias = alias;
            Type = type;
        }
    }
}
