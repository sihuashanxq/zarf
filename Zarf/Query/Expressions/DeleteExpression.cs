using System;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    public class DeleteExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public string ByKey { get; }

        public DbParameter ByKeyValue { get; }

        public Table Table { get; }

        public DeleteExpression(Table table,  string byKey, DbParameter byKeyValue)
        {
            Table = table;
            ByKey = byKey;
            ByKeyValue = byKeyValue;
        }
    }
}
