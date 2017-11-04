using System;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    public class DeleteExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public string Identity { get; }

        public DbParameter IdentityValue { get; }

        public Table Table { get; }

        public DeleteExpression(Table table,  string identity, DbParameter identityValue)
        {
            Table = table;
            Identity = identity;
            IdentityValue = identityValue;
        }
    }
}
