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

            if (query.Where != null && (query.Projections.Count != 0 || query.Sets.Count != 0))
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            if (query.QueryModel != null)
            {
                Context.QueryModelMapper.MapQueryModel(GetFirstParameter(methodCall.Arguments[1]), query.QueryModel);
            }

            MapParameterWithQuery(GetFirstParameter(methodCall.Arguments[1]), query);

            var key = new RelationExpressionCompiler(Context).Compile(methodCall.Arguments[1].UnWrap()).UnWrap();
            if (key.NodeType == ExpressionType.Lambda)
            {
                query.CombineCondtion(Expression.Not(key.As<LambdaExpression>().Body));
            }
            else
            {
                query.CombineCondtion(Expression.Not(key));
            }

            query.Projections.Clear();
            query.Projections.Add(Utils.ExpressionOne);
            query.QueryModel = new QueryEntityModel(methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body, typeof(bool));
            return new AllExpression(query);
        }
    }
}
