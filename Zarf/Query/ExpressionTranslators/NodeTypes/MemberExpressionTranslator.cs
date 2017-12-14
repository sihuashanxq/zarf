using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Entities;
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
            //这里是否需要返回一个这么复杂的一个QueryExpression
            //所有的东西都重新基于全新设计的ExpressionVisitor实现
            var objExp = Context.MemberAccessMapper.GetMappedExpression(mem.Member);
            if (objExp.Is<QueryExpression>())
            {
                return objExp;   //new {item.User.Id,} item.User
            }

            var typeOfMember = mem.Member.GetPropertyType();
            if (typeof(IInternalQuery).IsAssignableFrom(typeOfMember))
            {
                return new QueryExpression(typeOfMember, Context.ColumnCaching, Context.Alias.GetNewTable());
            }

            if (objExp.Is<ColumnExpression>())
            {
                objExp = objExp.As<ColumnExpression>().Query;
            }
            else
            {
                objExp = GetCompiledExpression(mem.Expression);
            }

            var query = objExp.As<QueryExpression>();
            if (query == null)
            {
                return EvalMemberValue(mem.Member, objExp, out var value) ? value : mem;
            }

            var col = Context.ColumnCaching.GetColumn(new QueryColumnCacheKey(query, mem.Member));
            if (col == null)
            {
                return new ColumnExpression(query, mem.Member);
            }

            while (query.Container != null)
            {
                var cachedCol = Context.ColumnCaching
                    .GetColumn(new QueryColumnCacheKey(query.Container, mem.Member))
                    ?.Clone();

                if (cachedCol != null)
                {
                    return cachedCol;
                }

                query = query.Container;
            }

            return new ColumnExpression(query, new Column(col.Column?.Name ?? col.Alias), col.Type);
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
