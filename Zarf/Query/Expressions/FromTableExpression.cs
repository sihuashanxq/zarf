using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Zarf.Entities;
using System.Reflection;
using Zarf.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarf.Query.Expressions
{
    public class FromTableExpression : Expression
    {
        public Table Table { get; protected set; }

        public string Alias { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public FromTableExpression(Type entityType, string alias = "")
        {
            Type = entityType;
            Alias = alias;

            var attribute = entityType.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (attribute == null)
            {
                Table = new Table(entityType.Name);
            }
            else
            {
                Table = new Table(attribute.Name, attribute.Schema.IsNullOrEmpty() ? "dbo" : attribute.Schema);
            }
        }
    }
}
