using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.Visitors;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class AggregateNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AggregateNodeHandler()
        {
            var methods = new[] { "Max", "Sum", "Min", "Average", "Count", "LongCount" };
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => methods.Contains(item.Name));
        }

        public AggregateNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            var query = Compile<SelectExpression>(methodCall.Arguments[0]);

            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());
            }

            foreach (var item in query.Projections)
            {
                if (item is AliasExpression alias && alias.Expression is SelectExpression)
                {
                    query = query.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());
                    break;
                }
            }

            return HandleNode(query, methodCall.Arguments.Count == 1 ? null : methodCall.Arguments[1], methodCall.Method);
        }

        public override SelectExpression HandleNode(SelectExpression select, Expression keySelector, MethodInfo method)
        {
            select.Limit = 1;
            select.Projections.Clear();
            select.Groups.Clear();
            select.Orders.Clear();

            if (keySelector == null)
            {
                var aggregateExpression = new AggregateExpression(method, null, select, QueryContext.AliasGenerator.GetNewColumn());

                select.AddProjection(aggregateExpression);
                select.QueryModel = new QueryEntityModel(select, aggregateExpression, method.ReturnType, select.QueryModel);
                QueryContext.ModelMapper.Map(aggregateExpression, select.QueryModel);

                return select;
            }

            var parameter = keySelector.GetParameters().FirstOrDefault();
            var modelExpression = new QueryModelExpandExpressionVisitor(QueryContext, select, parameter)
                .Visit(keySelector)
                .UnWrap()
                .As<LambdaExpression>()
                .Body;

            Utils.CheckNull(select, "query");

            select.QueryModel = new QueryEntityModel(select, modelExpression, method.ReturnType, select.QueryModel);

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            var selector = new AggreateExpressionVisitor(QueryContext, select).Visit(modelExpression);
            if (selector.Is<SelectExpression>())
            {
                throw new Exception("Cannot perform an aggregate function on an expression containing an aggregate or a subquery.");
            }

            if (selector.Is<AliasExpression>())
            {
                var alias = selector.As<AliasExpression>();
                var key = new AggregateExpression(method, alias.Expression, select, alias.Alias);

                select.AddProjection(key);
                QueryContext.BindingMaper.Map(modelExpression.As<MemberExpression>(), key);
                QueryContext.ExpressionMapper.Map(modelExpression, key);

                return select;
            }
            else if (selector.Is<ColumnExpression>())
            {
                var col = selector.As<ColumnExpression>();
                var key = new AggregateExpression(method, col, select, QueryContext.AliasGenerator.GetNewColumn());

                select.AddProjection(key);
                QueryContext.BindingMaper.Map(modelExpression.As<MemberExpression>(), key);
                QueryContext.ExpressionMapper.Map(modelExpression, key);

                return select;
            }
            else if (selector.NodeType != ExpressionType.Extension)
            {
                var key = new AggregateExpression(method, selector, select, QueryContext.AliasGenerator.GetNewColumn());

                select.AddProjection(key);
                QueryContext.BindingMaper.Map(modelExpression.As<MemberExpression>(), key);
                QueryContext.ExpressionMapper.Map(modelExpression, key);

                return select;
            }

            throw new NotImplementedException();
        }
    }
}
