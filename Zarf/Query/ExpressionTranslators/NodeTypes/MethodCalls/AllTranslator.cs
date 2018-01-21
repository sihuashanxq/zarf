using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AllTranslator : MethodTranslator
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
            var select = Compile<SelectExpression>(methodCall.Arguments[0]);
            var allExpression = new AllExpression(Translate(select, methodCall.Arguments[1], methodCall.Method));

            QueryContext.ExpressionMapper.Map(allExpression, Utils.ExpressionConstantTrue);

            return allExpression;
        }

        public override SelectExpression Translate(SelectExpression select, Expression predicate, MethodInfo method)
        {
            var parameter = predicate.GetParameters().FirstOrDefault();

            Utils.CheckNull(select, "query");

            QueryContext.QueryMapper.AddSelectExpression(parameter, select);
            QueryContext.QueryModelMapper.MapQueryModel(parameter, select.QueryModel);

            predicate = CreateRealtionCompiler(select).Compile(predicate);
            predicate = new RelationExpressionVisitor().Visit(predicate);
            predicate = new SubQueryModelRewriter(select, QueryContext).ChangeQueryModel(predicate);
            predicate = predicate.UnWrap();

            select.Projections.Clear();
            select.AddProjection(Utils.ExpressionConstantTrue);
            select.CombineCondtion(predicate.NodeType == ExpressionType.Lambda
                ? Expression.Not(predicate.As<LambdaExpression>().Body)
                : Expression.Not(predicate));

            select.QueryModel = new QueryEntityModel(select, predicate, typeof(bool), select.QueryModel);

            return select;
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(SelectExpression select)
        {
            return new RelationExpressionCompiler(QueryContext);
        }
    }
}
