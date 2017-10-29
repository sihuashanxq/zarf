using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    public class UpdateExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public List<DbParameter> DbParams { get; }

        public List<string> Columns { get; }

        public string ByKey { get; }

        public DbParameter ByKeyValue { get; }

        public Table Table { get; }

        public UpdateExpression(Table table, List<DbParameter> dbParams, List<string> columns, string byKey, DbParameter byKeyValue)
        {
            Table = table;
            DbParams = dbParams;
            Columns = columns;
            ByKey = byKey;
            ByKeyValue = ByKeyValue;
        }
    }
}
