using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

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

        public AggregateTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate( MethodCallExpression methodCall)
        {
            var rootQuery = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            Expression aggregateKey = null;

            if (methodCall.Arguments.Count == 2)
            {
                var keySelectorLambda = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();
                if (rootQuery.Projections.Count != 0 || rootQuery.Sets.Count != 0)
                {
                    rootQuery = rootQuery.PushDownSubQuery(Context.Alias.GetNewTable());
                }

                MapQuerySource( keySelectorLambda.Parameters.FirstOrDefault(), rootQuery);
                aggregateKey = Context
                    .ProjectionScanner
                    .Scan(Compiler.Compile, keySelectorLambda)
                    .FirstOrDefault()
                    .Expression;
            }
            else
            {
                aggregateKey = new ColumnExpression(rootQuery, null, methodCall.Method.ReturnType, "1");
            }

            var aggregate = new AggregateExpression(methodCall.Method, aggregateKey);

            rootQuery.Projections.Add(new ColumnDescriptor() { Expression = aggregate, Ordinal = rootQuery.Projections.Count });
            rootQuery.Result = new EntityResult(aggregate, methodCall.Method.ReturnType);

            return rootQuery;
        }
    }
}
