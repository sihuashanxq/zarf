using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes
{
    public class MemberNodeHandler : QueryNodeHandler<MemberExpression>
    {
        public MemberNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(MemberExpression mem)
        {
            var queryModel = QueryContext.ModelMapper.GetValue(mem.Expression);
            var modelExpression = queryModel?.GetModelExpression(mem.Member.DeclaringType);

            if (modelExpression != null)
            {
                var property = Expression.MakeMemberAccess(modelExpression, mem.Member);
                var propertyExpression = QueryContext.BindingMaper.GetValue(property);
                if (propertyExpression != null)
                {
                    if (propertyExpression.NodeType != ExpressionType.Extension)
                    {
                        propertyExpression = Compile(propertyExpression);
                    }

                    if (propertyExpression.Is<AliasExpression>())
                    {
                        var alias = propertyExpression.As<AliasExpression>();
                        var owner = QueryContext.SelectMapper.GetValue(alias);

                        if (owner.OuterSelect?.SubSelect == owner)
                        {
                            owner = owner.OuterSelect;
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

                    if (propertyExpression.Is<SelectExpression>())
                    {
                        var refQuery = propertyExpression.As<SelectExpression>();
                        if (refQuery.OuterSelect != null && refQuery.OuterSelect.SubSelect == refQuery)
                        {
                            return refQuery.OuterSelect;
                        }
                    }

                    return propertyExpression;
                }
            }

            var typeOfProperty = mem.Member.GetPropertyType();
            if (typeof(IInternalQuery).IsAssignableFrom(typeOfProperty))
            {
                return new SelectExpression(typeOfProperty, QueryContext.ExpressionMapper, QueryContext.AliasGenerator.GetNewTable());
            }

            var obj = Compile(mem.Expression);
            var select = obj.As<SelectExpression>();
            if (select == null)
            {
                if (Utils.TryEvaluateObjectMemberValue(mem.Member, obj, out var evalValue))
                {
                    return evalValue;
                }

                return mem;
            }

            return new ColumnExpression(select, mem.Member);
        }
    }
}
