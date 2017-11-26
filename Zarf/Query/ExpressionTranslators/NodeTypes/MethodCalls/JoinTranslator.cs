using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
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

        public JoinTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate( MethodCallExpression methodCall)
        {
            var rootQuery = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var joinQuery = Compiler.Compile(methodCall.Arguments[1]).As<QueryExpression>();

            if (rootQuery.Projections.Count != 0)
            {
                rootQuery = rootQuery.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            //有子查询选择了具体列 ，如 JOIN (SELECT Name,Age FROM User) AS B
            if (joinQuery.Projections.Count != 0 || joinQuery.Where != null || joinQuery.Sets.Count != 0)
            {
                joinQuery = joinQuery.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            var outer = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();
            var inner = methodCall.Arguments[3].UnWrap().As<LambdaExpression>();
            var selector = methodCall.Arguments[4].UnWrap().As<LambdaExpression>();

            MapQuerySource(outer.Parameters.FirstOrDefault(), rootQuery);
            MapQuerySource( inner.Parameters.FirstOrDefault(), joinQuery);
            MapQuerySource( selector.Parameters.FirstOrDefault(), rootQuery);
            MapQuerySource(selector.Parameters.LastOrDefault(), joinQuery);

            var left = Compiler.Compile(outer).UnWrap().As<LambdaExpression>().Body;
            var right = Compiler.Compile(inner).UnWrap().As<LambdaExpression>().Body;

            //只保留Selector中的Columns
            var entityNew = Compiler.Compile(selector).UnWrap();

            rootQuery.Projections.AddRange(Context.ProjectionScanner.Scan(entityNew));
            rootQuery.AddJoin(new JoinExpression(joinQuery, Expression.Equal(left, right), GetJoinType(rootQuery, joinQuery)));
            rootQuery.Result = new EntityResult(entityNew, methodCall.Method.ReturnType.GetCollectionElementType());

            return rootQuery;
        }

        private JoinType GetJoinType(QueryExpression left, QueryExpression right)
        {
            if (left.DefaultIfEmpty)
            {
                return right.DefaultIfEmpty ? JoinType.Full : JoinType.Right;
            }

            return right.DefaultIfEmpty ? JoinType.Left : JoinType.Inner;
        }
    }
}
