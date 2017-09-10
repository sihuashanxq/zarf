using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class JoinTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static JoinTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Join");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var rootQuery = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var joinQuery = transformVisitor.Visit(methodCall.Arguments[1]).As<QueryExpression>();

            if (rootQuery.Projections.Count != 0)
            {
                rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
            }

            //有子查询选择了具体列 ，如 JOIN (SELECT Name,Age FROM User) AS B
            if (joinQuery.Projections.Count != 0 || joinQuery.Where != null || joinQuery.Sets.Count != 0)
            {
                joinQuery = joinQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
            }

            var outer = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();
            var inner = methodCall.Arguments[3].UnWrap().As<LambdaExpression>();
            var selector = methodCall.Arguments[4].UnWrap().As<LambdaExpression>();

            context.QuerySourceProvider.AddSource(outer.Parameters.First(), rootQuery);
            context.QuerySourceProvider.AddSource(inner.Parameters.First(), joinQuery);
            context.QuerySourceProvider.AddSource(selector.Parameters.First(), rootQuery);
            context.QuerySourceProvider.AddSource(selector.Parameters.Last(), joinQuery);

            var left = transformVisitor.Visit(outer).UnWrap().As<LambdaExpression>().Body;
            var right = transformVisitor.Visit(inner).UnWrap().As<LambdaExpression>().Body;
            //只保留Selector中的Columns
            var entityNew = transformVisitor.Visit(selector).UnWrap();
            var projections = context.ProjectionScanner.Scan(entityNew);

            rootQuery.AddJoin(new JoinExpression(joinQuery, Expression.Equal(left, right), GetJoinType(rootQuery, joinQuery)));

            foreach (var item in projections)
            {
                if (!item.Is<FromTableExpression>())
                {
                    rootQuery.Projections.Add(item);
                    continue;
                }

                rootQuery.Projections.AddRange(item.As<FromTableExpression>().GenerateColumns());
            }

            rootQuery.Result = new EntityResult(entityNew, methodCall.Method.ReturnType.GetCollectionElementType());
            return rootQuery;
        }

        private JoinType GetJoinType(QueryExpression left, QueryExpression right)
        {
            var joinType = JoinType.Inner;

            if (left.DefaultIfEmpty)
            {
                joinType = right.DefaultIfEmpty ? JoinType.Full : JoinType.Right;
            }
            else
            {
                joinType = right.DefaultIfEmpty ? JoinType.Left : JoinType.Inner;
            }

            return joinType;
        }
    }
}
