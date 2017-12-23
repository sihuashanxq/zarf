using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.Methods;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MemberExpressionTranslator : Translator<MemberExpression>
    {
        public MemberExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MemberExpression mem)
        {
            var queryModel = Context.QueryModelMapper.GetQueryModel(mem.Expression);
            var modelExpression = queryModel?.GetModelExpression(mem.Member.DeclaringType);

            if (modelExpression != null)
            {
                var property = Expression.MakeMemberAccess(modelExpression, mem.Member);
                var propertyExpression = Context.MemberBindingMapper.GetMapedExpression(property);
                if (propertyExpression.NodeType != ExpressionType.Extension)
                {
                    propertyExpression = GetCompiledExpression(propertyExpression);
                }

                if (propertyExpression.Is<AliasExpression>())
                {
                    var refrence = propertyExpression.As<AliasExpression>();
                    var refQuery = Context.ProjectionOwner.GetQuery(refrence);
                    if (refQuery.Outer?.SubQuery == refQuery)
                    {
                        refQuery = refQuery.Outer;
                    }

                    if (refQuery.QueryModel.ContainsModel(queryModel.Model))
                    {
                        return propertyExpression;
                    }

                    return new ColumnExpression(refQuery, new Column(refrence.Alias), refrence.Type);
                }

                if (propertyExpression.Is<QueryExpression>())
                {
                    var refQuery = propertyExpression.As<QueryExpression>();
                    if (refQuery.Outer != null && refQuery.Outer.SubQuery == refQuery)
                    {
                        return refQuery.Outer;
                    }
                }

                return propertyExpression;
            }

            var typeOfProperty = mem.Member.GetPropertyType();
            if (typeof(IInternalQuery).IsAssignableFrom(typeOfProperty))
            {
                return new QueryExpression(typeOfProperty, Context.ColumnCaching, Context.Alias.GetNewTable());
            }

            var obj = GetCompiledExpression(mem.Expression);
            var query = obj.As<QueryExpression>();
            if (query == null)
            {
                return EvalMemberValue(mem.Member, obj, out var value) ? value : mem;
            }

            return new ColumnExpression(query, mem.Member);
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
            if (objExp != null && !objExp.Is<ConstantExpression>())
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
