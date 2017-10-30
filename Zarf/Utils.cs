using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf
{
    public static class Utils
    {
        public static void CheckNull(object v, string name)
        {
            if (v == null)
            {
                throw new NullReferenceException($"{name} is null!");
            }
        }

        public static Expression ExpressionTrue = Expression.Equal(Expression.Constant(true), Expression.Constant(true));

        public static Expression ExpressionFalse = Expression.Equal(Expression.Constant(true), Expression.Constant(false));

        public static Dictionary<ExpressionType, string> OperatorMap = new Dictionary<ExpressionType, string>()
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        public static Dictionary<string, string> AggregateFunctionMap = new Dictionary<string, string>()
        {
            {"Min","Min" },
            {"Max","Max" },
            {"Sum","Sum" },
            {"Average","Avg" },
            {"Count","Count" },
            {"LongCount","Count_Big" }
        };
    }
}
