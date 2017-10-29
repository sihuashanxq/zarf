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

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, IQueryCompiler queryCompiler)
        {
            var rootQuery = queryCompiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var lambda = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (rootQuery.Sets.Count != 0)
            {
                rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
            }

            context.QuerySourceProvider.AddSource(lambda.Parameters.First(), rootQuery);

            rootQuery.Orders.Add(new OrderExpression(
                context
                .ProjectionScanner
                .Scan(queryCompiler.Compile(lambda))
                .Select(item => item.Expression)
                .OfType<ColumnExpression>(),
                GetOrderType(methodCall)
                )
            );

            return rootQuery;
        }
    }
}
