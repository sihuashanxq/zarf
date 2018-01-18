using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AllTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AllTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "All");
        }

        public AllTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var allExpression = new AllExpression(Translate(query, methodCall.Arguments[1]));

            Context.ExpressionMapper.Map(allExpression, Utils.ExpressionConstantTrue);

            return allExpression;
        }

        public virtual QueryExpression Translate(QueryExpression query, Expression predicate)
        {
            var parameter = predicate.GetParameters().FirstOrDefault();

            Utils.CheckNull(query, "query");

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            predicate = CreateRealtionCompiler(query).Compile(predicate);
            predicate = new RelationExpressionVisitor().Visit(predicate);
            predicate = new SubQueryModelRewriter(query, Context).ChangeQueryModel(predicate);
            predicate = predicate.UnWrap();

            query.Projections.Clear();
            query.AddProjection(Utils.ExpressionConstantTrue);
            query.CombineCondtion(predicate.NodeType == ExpressionType.Lambda
                ? Expression.Not(predicate.As<LambdaExpression>().Body)
                : Expression.Not(predicate));

            query.QueryModel = new QueryEntityModel(query, predicate, typeof(bool), query.QueryModel);

            return query;
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(QueryExpression query)
        {
            return new RelationExpressionCompiler(Context);
        }
    }
}
