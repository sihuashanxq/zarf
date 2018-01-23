using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class OrderByTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static OrderByTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods
                .Where(
                    item =>
                        item.Name == "ThenBy" ||
                        item.Name == "OrderBy" ||
                        item.Name == "ThenByDescending" ||
                        item.Name == "OrderByDescending"
                );
        }

        public OrderByTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression keySelector, MethodInfo method)
        {
            var parameter = keySelector.GetParameters().FirstOrDefault();
            if (select.Sets.Count != 0)
            {
                select = select.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());
            }

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            var sortKey = GetSortKey(select, keySelector);
            if (sortKey == null)
            {
                throw new Exception("not found order by field!");
            }

            select.Orders.Add(
                new OrderExpression(new[] { sortKey },
                method.Name.EndsWith("Descending") ? OrderDirection.Desc : OrderDirection.Asc));

            return select;
        }

        protected virtual ColumnExpression GetSortKey(SelectExpression select, Expression keySelector)
        {
            var sortKey = new RelationExpressionVisitor(QueryContext).Visit(keySelector.UnWrap().As<LambdaExpression>().Body);
            if (sortKey is AliasExpression alias)
            {
                var rSelect = QueryContext.SelectMapper.GetValue(alias);
                if (!select.ContainsSelectExpression(rSelect))
                {
                    throw new Exception("order by field must in current select!");
                }

                if (alias.Expression is ColumnExpression)
                {
                    return alias.Expression.As<ColumnExpression>();
                }

                if (rSelect != select)
                {
                    return new ColumnExpression(rSelect, new Column(alias.Alias), alias.Type);
                }
            }

            return sortKey as ColumnExpression;
        }
    }
}
