using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query.Expressions
{
    public class GroupExpression : Expression
    {
        internal static readonly Type ObjectType = typeof(object);

        public IEnumerable<ColumnExpression> Columns { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => ObjectType;

        public GroupExpression(IEnumerable<ColumnExpression> columns)
        {
            Columns = columns;
        }
    }
}