using System;
using System.Linq.Expressions;

using System.Collections.Generic;
using Zarf.Metadata.Entities;

namespace Zarf.Query.Expressions
{
    public class DeleteExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public string PrimaryKey { get; }

        public IEnumerable<DbParameter> PrimaryKeyValues { get; }

        public Table Table { get; }

        public DeleteExpression(Table table, string primary, IEnumerable<DbParameter> primaryKeyValues)
        {
            Table = table;
            PrimaryKey = primary;
            PrimaryKeyValues = primaryKeyValues;
        }
    }
}
