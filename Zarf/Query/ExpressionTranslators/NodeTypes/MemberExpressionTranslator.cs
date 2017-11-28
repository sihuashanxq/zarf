using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MemberExpressionTranslator : Translator<MemberExpression>
    {
        public MemberExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate(MemberExpression mem)
        {
            //new {item.User.Id,} item.User
            var objExp = Context.EntityMemberMappingProvider.GetExpression(mem.Member);
            if (objExp != null)
            {
                return objExp;
            }

            var typeOfMember = mem.Member.GetPropertyType();
            if (typeof(IInternalDbQuery).IsAssignableFrom(typeOfMember))
            {
                return new QueryExpression(typeOfMember, Context.Alias.GetNewTable());
            }

            objExp = GetCompiledExpression(mem.Expression);
            if (EvalMemberValue(mem.Member, objExp, out var value))
            {
                return value;
            }

            if (objExp.Is<QueryExpression>())
            {
                return new ColumnExpression(objExp.Cast<QueryExpression>(), mem.Member);
            }

            return mem;
        }

        /// <summary>
        /// 表达式求值  string.Empty 
        /// </summary>
        /// <param name="memberInfo">类成员 FieldInfo|Property</param>
        /// <param name="objExp">所属实例</param>
        /// <returns></returns>
        private bool EvalMemberValue(MemberInfo memberInfo, Expression objExp, out Expression value)
        {
            object obj = null;

            if (!objExp.Is<ConstantExpression>())
            {
                value = null;
                return false;
            }

            if (objExp.Is<ConstantExpression>())
            {
                obj = objExp.Cast<ConstantExpression>().Value;
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
