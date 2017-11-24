using System.Collections.Generic;
using System.Linq.Expressions;

using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;

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

        public static bool IsNullValueConstant(this Expression node)
        {
            if (node.Is<ConstantExpression>())
            {
                return node.As<ConstantExpression>().Value == null;
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

        public static IEnumerable<ColumnExpression> GenerateColumns(this FromTableExpression table)
        {
            return ITranslatorExtensions.GenerateTableColumns(null, table);
        }
    }
}
