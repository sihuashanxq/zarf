using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    public class InsertExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public List<DbParameter> DbParams { get; }

        public List<string> Columns { get; }

        public Table Table { get; }

        public bool HasAutoIncrement { get; }

        public InsertExpression(Table table, List<DbParameter> dbParams, List<string> columns, bool hasAutoIncrement = false)
        {
            Table = table;
            DbParams = dbParams;
            Columns = columns;
            HasAutoIncrement = hasAutoIncrement;
        }
    }
}
