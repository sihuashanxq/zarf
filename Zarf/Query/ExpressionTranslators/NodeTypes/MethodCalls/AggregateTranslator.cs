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

            if (methodCall.Arguments.Count == 1)
            {
                var col = new ColumnExpression(query, new Column(Context.Alias.GetNewColumn()), methodCall.Method.ReturnType);

                query.AddProjection(new AggregateExpression(methodCall.Method, col, query, col.Column.Name));

                return query;
            }

            var parameter = methodCall.Arguments[1].GetParameters().FirstOrDefault();
            var modelExpression = new ModelRefrenceExpressionVisitor(Context, query, parameter)
                .Visit(methodCall.Arguments[1])
                .UnWrap()
                .As<LambdaExpression>()
                .Body;

            Utils.CheckNull(query, "query");

            query.QueryModel = new QueryEntityModel(modelExpression, methodCall.Method.ReturnType, query.QueryModel);

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            query.Projections.Clear();

            var selector = new AggreateExpressionVisitor(Context, query).Compile(modelExpression);
            if (selector.Is<QueryExpression>())
            {
                throw new Exception("Cannot perform an aggregate function on an expression containing an aggregate or a subquery.");
            }

            if (selector.Is<AliasExpression>())
            {
                var alias = selector.As<AliasExpression>();
                var key = new AggregateExpression(methodCall.Method, alias.Expression, query, alias.Alias);

                query.AddProjection(key);
                Context.MemberBindingMapper.Map(modelExpression.As<MemberExpression>(), key);
                var s = Context.DbContextParts.CommandTextBuilder.Build(query);
                return query;
            }

            if (selector.Is<ColumnExpression>())
            {
                var col = selector.As<ColumnExpression>();
                var key = new AggregateExpression(methodCall.Method, col, query, Context.Alias.GetNewColumn());

                query.AddProjection(key);
                Context.MemberBindingMapper.Map(modelExpression.As<MemberExpression>(), key);
                var s = Context.DbContextParts.CommandTextBuilder.Build(query);
                return query;
            }

            if (selector.NodeType != ExpressionType.Extension)
            {
                var key = new AggregateExpression(methodCall.Method, selector, query, Context.Alias.GetNewColumn());

                query.AddProjection(key);

                if (!selector.Is<ConstantExpression>())
                {
                    Context.MemberBindingMapper.Map(modelExpression.As<MemberExpression>(), key);
                }

                var s = Context.DbContextParts.CommandTextBuilder.Build(query);

                return query;
            }

            throw new NotImplementedException();
        }
    }

    public class AggreateExpressionVisitor : QueryCompiler
    {
        public QueryExpression Query { get; }

        public AggreateExpressionVisitor(IQueryContext context, QueryExpression query) : base(context)
        {
            Query = query;
        }

        public override Expression Compile(Expression exp)
        {
            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                return VisitMember(exp.As<MemberExpression>());
            }

            return base.Compile(exp);
        }

        /// <summary>
        /// 聚合引用其他表的列,拷贝引用表
        /// </summary>
        protected override Expression VisitMember(MemberExpression mem)
        {
            QueryExpression query = null;
            var expression = base.Compile(mem);
            var queryModel = Context.QueryModelMapper.GetQueryModel(mem.Expression);

            if (queryModel != null)
            {
                var modelExpression = queryModel.GetModelExpression(mem.Expression.Type.GetModelElementType());
                var refrence = Context.MemberBindingMapper.GetMapedExpression(Expression.MakeMemberAccess(modelExpression, mem.Member));
                if (refrence != null)
                {
                    query = Context.ProjectionOwner.GetQuery(refrence);
                }
            }

            query = query ?? expression.As<ColumnExpression>()?.Query;

            if (query == null || query == Query)
            {
                return expression;
            }

            var clonedQuery = query.Clone();

            Query.AddProjection(expression);
            Query.AddJoin(new JoinExpression(clonedQuery, null, JoinType.Cross));
            Query.Groups.Add(new GroupExpression(new[] { expression.As<ColumnExpression>() }));

            clonedQuery.Projections.Clear();

            return expression;
        }
    }
}
