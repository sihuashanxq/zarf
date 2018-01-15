using System;
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
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => methods.Contains(item.Name));
        }

        public AggregateTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);

            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            foreach (var item in query.Projections)
            {
                if (item is AliasExpression alias && alias.Expression is QueryExpression)
                {
                    query = query.PushDownSubQuery(Context.Alias.GetNewTable());
                    break;
                }
            }

            return Translate(query, methodCall.Arguments.Count == 1 ? null : methodCall.Arguments[1], methodCall.Method);
        }

        public virtual QueryExpression Translate(QueryExpression query, Expression keySelector, MethodInfo method)
        {
            query.Projections.Clear();
            query.Groups.Clear();
            query.Orders.Clear();
            query.Limit = 1;

            if (keySelector == null)
            {
                var columnExpression = new ColumnExpression(query, new Column(Context.Alias.GetNewColumn()), method.ReturnType);
                var aggregateExpression = new AggregateExpression(method, columnExpression, query, columnExpression.Column.Name);

                query.AddProjection(aggregateExpression);
                query.QueryModel = new QueryEntityModel(query, aggregateExpression, method.ReturnType, query.QueryModel);

                Context.QueryModelMapper.MapQueryModel(aggregateExpression, query.QueryModel);

                return query;
            }

            var parameter = keySelector.GetParameters().FirstOrDefault();
            var modelExpression = new ModelRefrenceExpressionVisitor(Context, query, parameter)
                .Visit(keySelector)
                .UnWrap()
                .As<LambdaExpression>()
                .Body;

            Utils.CheckNull(query, "query");

            query.QueryModel = new QueryEntityModel(query, modelExpression, method.ReturnType, query.QueryModel);

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            var selector = new AggreateExpressionVisitor(Context, query).Visit(modelExpression);
            if (selector.Is<QueryExpression>())
            {
                throw new Exception("Cannot perform an aggregate function on an expression containing an aggregate or a subquery.");
            }

            if (selector.Is<AliasExpression>())
            {
                var alias = selector.As<AliasExpression>();
                var key = new AggregateExpression(method, alias.Expression, query, alias.Alias);

                query.AddProjection(key);
                Context.MemberBindingMapper.Map(modelExpression.As<MemberExpression>(), key);
                Context.ExpressionMapper.Map(modelExpression, key);

                return query;
            }
            else if (selector.Is<ColumnExpression>())
            {
                var col = selector.As<ColumnExpression>();
                var key = new AggregateExpression(method, col, query, Context.Alias.GetNewColumn());

                query.AddProjection(key);
                Context.MemberBindingMapper.Map(modelExpression.As<MemberExpression>(), key);
                Context.ExpressionMapper.Map(modelExpression, key);

                return query;
            }
            else if (selector.NodeType != ExpressionType.Extension)
            {
                var key = new AggregateExpression(method, selector, query, Context.Alias.GetNewColumn());

                query.AddProjection(key);
                Context.MemberBindingMapper.Map(modelExpression.As<MemberExpression>(), key);
                Context.ExpressionMapper.Map(modelExpression, key);

                return query;
            }

            throw new NotImplementedException();
        }
    }
}
