using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Mapping;
using System.Reflection;
using System.Linq;

namespace Zarf.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression UnWrap(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Quote)
            {
                return UnWrap(((UnaryExpression)expression)?.Operand);
            }

            return expression;
        }

        public static bool IsNullValueConstant(this Expression expression)
        {
            if (expression.Is<ConstantExpression>())
            {
                return expression.As<ConstantExpression>().Value == null;
            }

            return false;
        }

        public static T GetValue<T>(this ConstantExpression constant)
        {
            if (constant == null)
            {
                return default(T);
            }

            return constant.Value.Cast<T>();
        }

        public static IEnumerable<Expression> GenerateColumns(this FromTableExpression table)
        {
            var entityType = EntityTypeDescriptorFactory.Factory.Create(table.Type);
            foreach (var member in entityType.GetWriteableMembers())
            {
                yield return new ColumnExpression(table, member, member.Name);
            }
        }
    }
}
