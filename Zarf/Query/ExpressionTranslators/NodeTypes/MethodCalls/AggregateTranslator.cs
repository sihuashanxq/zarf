using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AggregateTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AggregateTranslator()
        {
            var methods = new[] { "Max", "Sum", "Min", "Average", "Count", "LongCount" };
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => methods.Contains(item.Name));
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var rootQuery = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            Expression aggregateKey = null;

            if (methodCall.Arguments.Count == 2)
            {
                var keySelectorLambda = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();
                if (rootQuery.ProjectionCollection.Count != 0 || rootQuery.Sets.Count != 0)
                {
                    rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable());
                }

                context.QuerySourceProvider.AddSource(keySelectorLambda.Parameters.FirstOrDefault(), rootQuery);
                aggregateKey = context
                    .ProjectionScanner
                    .Scan(transformVisitor.Visit, keySelectorLambda)
                    .FirstOrDefault()
                    .Expression;
            }

            var aggregate = new AggregateExpression(methodCall.Method, aggregateKey);

            rootQuery.ProjectionCollection.Add(new Projection() { Expression = aggregate, Query = rootQuery });
            rootQuery.Result = new EntityResult(aggregate, methodCall.Method.ReturnType);

            return rootQuery;
        }
    }
}
