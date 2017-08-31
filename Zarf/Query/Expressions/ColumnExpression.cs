using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarf.Query.Expressions
{
    public class ColumnExpression : AliasExpression
    {
        public Column Column { get; }

        public MemberInfo Member { get; }

        public FromTableExpression FromTable { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public ColumnExpression(FromTableExpression table, MemberInfo member, string alias = "")
            : base(alias)
        {
            FromTable = table;
            Member = member;

            if (member != null)
            {
                var attribute = Member.GetCustomAttribute<ColumnAttribute>();
                var columnName = Member.Name;
                if (attribute != null)
                {
                    columnName = attribute.Name;
                }

                Type = member.GetMemberInfoType();
                Column = new Column(columnName);
                Alias = alias;
            }
        }

        public ColumnExpression(FromTableExpression table, Column column, Type refType, string alias = "")
            : base(alias)
        {
            FromTable = table;
            Column = column;
            Type = refType;
        }
    }
}
