using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries.ExpressionTranslators.NodeTypes
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
                if (propertyExpression != null)
                {
                    if (propertyExpression.NodeType != ExpressionType.Extension)
                    {
                        propertyExpression = GetCompiledExpression(propertyExpression);
                    }

                    if (propertyExpression.Is<AliasExpression>())
                    {
                        var alias = propertyExpression.As<AliasExpression>();
                        var owner = Context.ProjectionOwner.GetQuery(alias);

                        if (owner.Outer?.SubQuery == owner)
                        {
                            owner = owner.Outer;
                        }

                        if (owner.QueryModel == queryModel)
                        {
                            return alias.Expression;
                        }

                        if (owner.QueryModel.ContainsModel(queryModel.Model))
                        {
                            return propertyExpression;
                        }

                        return new ColumnExpression(owner, new Column(alias.Alias), alias.Type);
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
                if (Utils.TryEvaluateObjectMemberValue(mem.Member, obj, out var evalValue))
                {
                    return evalValue;
                }

                return mem;
            }

            return new ColumnExpression(query, mem.Member);
        }
    }
}
