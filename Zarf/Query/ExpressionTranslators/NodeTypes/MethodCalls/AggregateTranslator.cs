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

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var column = new ColumnExpression(query, null, methodCall.Method.ReturnType, "1");

            if (query.Projections.Count != 0 || query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            if (methodCall.Arguments.Count == 2)
            {
                RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[1]), query);
                column = GetColumns(GetCompiledExpression(methodCall.Arguments[1])).FirstOrDefault().Expression.As<ColumnExpression>();
                column.Alias = string.Empty;
            }

            var aggregate = new AggregateExpression(methodCall.Method, column);
            query.Result = new EntityResult(aggregate, methodCall.Method.ReturnType);
            query.Projections.Add(new ColumnDescriptor()
            {
                Expression = aggregate,
                Ordinal = query.Projections.Count
            });

            return query;
        }
    }
}
