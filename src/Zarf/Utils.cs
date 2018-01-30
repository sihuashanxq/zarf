using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;

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

        public static Expression ExpressionConstantTrue = Expression.Constant(true);

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

        /// <summary>
        /// 属性string.Empty 
        /// </summary>
        public static bool TryEvaluateObjectMemberValue(MemberInfo memberInfo, Expression objExpression, out Expression value)
        {
            var obj = null as object;

            if (objExpression != null && !objExpression.Is<ConstantExpression>())
            {
                value = null;
                return false;
            }

            if (objExpression.Is<ConstantExpression>())
            {
                obj = objExpression.Cast<ConstantExpression>().Value;
            }

            var field = memberInfo.As<FieldInfo>();
            if (field != null)
            {
                value = Expression.Constant(field.GetValue(obj));
                return true;
            }

            var property = memberInfo.As<PropertyInfo>();
            if (property != null && property.CanRead)
            {
                value = Expression.Constant(property.GetValue(obj));
                return true;
            }

            value = null;
            return false;
        }
    }
}
