using System;
using System.Linq.Expressions;

namespace Zarf.Queries.Expressions
{
    public class ColumnRefrenceExpression : SourceExpression
    {
        public ColumnExpression Column { get; }

        public QueryExpression Query { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Column.Type;

        public ColumnRefrenceExpression(ColumnExpression column, QueryExpression query, Expression source)
            : base(source)
        {
            Column = column;
            Query = query;
        }
    }
}
