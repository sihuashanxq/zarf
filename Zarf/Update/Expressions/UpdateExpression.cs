using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Metadata.Entities;

namespace Zarf.Query.Expressions
{
    public class UpdateExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public IEnumerable<DbParameter> DbParams { get; }

        public IEnumerable<string> Columns { get; }

        public string Identity { get; }

        public DbParameter IdentityValue { get; }

        public Table Table { get; }

        public UpdateExpression(
            Table table,
            IEnumerable<DbParameter> parameters,
            IEnumerable<string> columns,
            string identity,
            DbParameter identityValue)
        {
            Table = table;
            DbParams = parameters;
            Columns = columns;
            Identity = identity;
            IdentityValue = identityValue;
        }
    }
}
