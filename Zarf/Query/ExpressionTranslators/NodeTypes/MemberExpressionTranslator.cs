using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MemberExpressionTranslator : Translator<MemberExpression>
    {
        public override Expression Translate(QueryContext context, MemberExpression memExpression, ExpressionVisitor transformVisitor)
        {
            //new {item.User.Id,} item.User
            if (context.MemberSource.TryGetValue(memExpression.Member, out QueryExpression query))
            {
                return query;
            }

            var typeInfo = memExpression.Member.GetMemberInfoType();
            if (typeof(IDataQuery).IsAssignableFrom(typeInfo))
            {
                return new QueryExpression(typeInfo, context.CreateAlias());
            }

            var belongInstance = transformVisitor.Visit(memExpression.Expression);
            var evalValue = TryEvalMemberValue(memExpression.Member, belongInstance);
            if (evalValue != null)
            {
                return evalValue;
            }

            if (belongInstance.Is<QueryExpression>())
            {
                var columnExpression = new ColumnExpression(belongInstance.Cast<QueryExpression>(), memExpression.Member);
                context.Projections.Add(columnExpression);
                return columnExpression;
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
