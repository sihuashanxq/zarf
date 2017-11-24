using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MemberExpressionTranslator : Translator<MemberExpression>
    {
        public override Expression Translate(IQueryContext context, MemberExpression memExpression, IQueryCompiler queryCompiler)
        {
            //new {item.User.Id,} item.User
            var exp = context.EntityMemberMappingProvider.GetExpression(memExpression.Member);
            if (exp != null)
            {
                return exp;
            }

            var typeInfo = memExpression.Member.GetPropertyType();
            if (typeof(IInternalDbQuery).IsAssignableFrom(typeInfo))
            {
                return new QueryExpression(typeInfo, context.Alias.GetNewTable());
            }

            var belongInstance = queryCompiler.Compile(memExpression.Expression);
            var value = TryEvalMemberValue(memExpression.Member, belongInstance);
            if (value != null)
            {
                return value;
            }

            if (belongInstance.Is<QueryExpression>())
            {
                return new ColumnExpression(belongInstance.Cast<QueryExpression>(), memExpression.Member);
            }

            return memExpression;
        }

        /// <summary>
        /// 表达式求值  string.Empty 
        /// </summary>
        /// <param name="memberInfo">类成员 FieldInfo|Property</param>
        /// <param name="belongInstance">所属实例</param>
        /// <returns></returns>
        private Expression TryEvalMemberValue(MemberInfo memberInfo, Expression belongInstance)
        {
            if (belongInstance != null && !belongInstance.Is<ConstantExpression>())
            {
                return null;
            }

            object instanceObj = null;

            if (belongInstance.Is<ConstantExpression>())
            {
                instanceObj = belongInstance.Cast<ConstantExpression>().Value;
            }

            if (memberInfo.MemberType == MemberTypes.Field)
            {
                return Expression.Constant(memberInfo.Cast<FieldInfo>().GetValue(instanceObj));
            }

            if (memberInfo.MemberType == MemberTypes.Property)
            {
                var property = memberInfo.Cast<PropertyInfo>();
                if (property.GetMethod == null)
                {
                    return null;
                }

                return Expression.Constant(property.GetMethod.Invoke(instanceObj, null));
            }

            return null;
        }
    }
}
