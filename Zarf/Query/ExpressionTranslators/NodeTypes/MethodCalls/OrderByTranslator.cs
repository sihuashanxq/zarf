using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class OrderByTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static OrderByTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods
                .Where(
                    item =>
                        item.Name == "ThenBy" ||
                        item.Name == "OrderBy" ||
                        item.Name == "ThenByDescending" ||
                        item.Name == "OrderByDescending"
                );
        }

        private OrderType GetOrderType(MethodCallExpression methodCall)
        {
            return methodCall.Method.Name == "OrderBy" || methodCall.Method.Name == "ThenBy"
                ? OrderType.Asc
                : OrderType.Desc;
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var selector = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(context.AliasGenerator.GetNewTableAlias(), context.UpdateRefrenceSource);
            }

            context.QuerySourceProvider.AddSource(selector.Parameters.First(), query);

            var orderByKey = transformVisitor.Visit(selector);

            query.Orders.Add(new OrderExpression(
                context.ProjectionFinder.Find<ColumnExpression>(orderByKey),
                GetOrderType(methodCall)
                )
            );

            return query;
        }
    }
}
