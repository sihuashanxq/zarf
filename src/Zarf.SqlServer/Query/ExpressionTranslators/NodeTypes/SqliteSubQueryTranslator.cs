using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;
using Zarf.Query.ExpressionTranslators.NodeTypes;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes
{
    public class SqliteSubQueryTranslator : SubQueryTranslator
    {
        protected Dictionary<string, ITranslator> _translators;

        public SqliteSubQueryTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            _translators = new Dictionary<string, ITranslator>()
            {

            };
        }

        public override SelectExpression Translate(SelectExpression select, Expression expression, MethodInfo method)
        {
            if (method.Name == "ToList")
            {
                return select;
            }

            return base.Translate(select, expression, method);
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == "All")
            {
                var selectExpression = new AllTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
                var allExpression = new AllExpression(selectExpression);

                return allExpression;
            }

            if (methodCall.Method.Name == "Any")
            {
                var selectExpression = new AnyTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
                var anyExpression = new AnyExpression(selectExpression);

                QueryContext.ExpressionMapper.Map(anyExpression, Utils.ExpressionConstantTrue);

                return anyExpression;
            }

            if (methodCall.Method.Name == "Select")
            {
                if (typeof(IJoinQuery).IsAssignableFrom(methodCall.Method.DeclaringType))
                {
                    select = new JoinSelectTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
                }
                else
                {
                    select = new SelectTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
                }
            }

            if (methodCall.Method.Name == "Where")
            {
                select = new WhereTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
            }

            if (new[] { "Count", "LongCount", "Sum", "Max", "Min", "Average" }.Contains(methodCall.Method.Name))
            {
                select = new AggregateTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments.Count == 0 ? null : methodCall.Arguments[0], methodCall.Method);
            }

            if (new[] { "First", "FirstOrDefault", "Single", "SingleOrDefault" }.Contains(methodCall.Method.Name))
            {
                select = new FirstTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments.Count == 1 ? methodCall.Arguments[0] : null, methodCall.Method);
            }

            if (methodCall.Method.Name == "Skip")
            {
                select = new SkipTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
            }

            if (methodCall.Method.Name == "Take")
            {
                select = new TakeTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
            }

            if (methodCall.Method.Name == "OrderBy")
            {
                select = new OrderByTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodCall.Method.Name == "GroupBy")
            {
                select = new GroupByTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodCall.Method.Name == "Union" || methodCall.Method.Name == "Concat")
            {
                select = new UnionTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodCall.Method.Name == "Except")
            {
                select = new ExceptTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodCall.Method.Name == "Intersect")
            {
                select = new IntersectTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            return base.Translate(methodCall);
        }
    }
}
