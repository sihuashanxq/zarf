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

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var joinQuery = GetCompiledExpression<QueryExpression>(methodCall.Arguments[1]);

            if (query.Projections.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            //有子查询选择了具体列 ，如 JOIN (SELECT Name,Age FROM User) AS B
            if (joinQuery.Projections.Count != 0 || joinQuery.Where != null || joinQuery.Sets.Count != 0)
            {
                joinQuery = joinQuery.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[2]), query);
            RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[3]), joinQuery);
            RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[4]), query);
            RegisterQuerySource(GetLastLambdaParameter(methodCall.Arguments[4]), joinQuery);

            var left = GetCompiledExpression(methodCall.Arguments[2]).UnWrap().As<LambdaExpression>().Body;
            var right = GetCompiledExpression(methodCall.Arguments[3]).UnWrap().As<LambdaExpression>().Body;

            //只保留Selector中的Columns
            var newEntity = GetCompiledExpression(methodCall.Arguments[4]).UnWrap();

            query.Projections.AddRange(Context.ProjectionScanner.Scan(newEntity));
            query.AddJoin(new JoinExpression(joinQuery, Expression.Equal(left, right), GetJoinType(query, joinQuery)));
            query.Result = new EntityResult(newEntity, methodCall.Method.ReturnType.GetCollectionElementType());

            return query;
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
