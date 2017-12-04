using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf
{
    public static class Utils
    {
        const string AnonymouseTypePrefix = "<>f__AnonymousType";

        public static void CheckNull(object v, string name)
        {
            if (v == null)
            {
                throw new NullReferenceException($"{name} is null!");
            }
        }

        /// <summary>
        /// 是否匿名类型
        /// </summary>
        public static bool IsAnonymouseType(Type type)
        {
            return type.Name.Contains(AnonymouseTypePrefix);
        }

        public static Expression ExpressionTrue = Expression.Equal(Expression.Constant(true), Expression.Constant(true));

        public static Expression ExpressionFalse = Expression.Equal(Expression.Constant(true), Expression.Constant(false));

        public static Expression ExpressionOne = Expression.Constant(1);

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
    }
}
