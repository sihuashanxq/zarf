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

        public OrderByTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        private OrderType GetOrderType(MethodCallExpression methodCall)
        {
            return methodCall.Method.Name == "OrderBy" || methodCall.Method.Name == "ThenBy"
                ? OrderType.Asc
                : OrderType.Desc;
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var rootQuery = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var lambda = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (rootQuery.Sets.Count != 0)
            {
                rootQuery = rootQuery.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            MapQuerySource(lambda.Parameters.LastOrDefault(), rootQuery);

            rootQuery.Orders.Add(new OrderExpression(
                Context
                .ProjectionScanner
                .Scan(Compiler.Compile(lambda))
                .Select(item => item.Expression)
                .OfType<ColumnExpression>(),
                GetOrderType(methodCall)
                )
            );

            return rootQuery;
        }
    }
}
