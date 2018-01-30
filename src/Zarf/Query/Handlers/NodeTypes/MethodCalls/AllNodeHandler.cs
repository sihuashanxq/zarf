using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.Visitors;
using Zarf.Query.Internals;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class AllNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AllNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "All");
        }

        public AllNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            var select = Compile<SelectExpression>(methodCall.Arguments[0]);
            var allExpression = new AllExpression(HandleNode(select, methodCall.Arguments[1], methodCall.Method));

            QueryContext.ExpressionMapper.Map(allExpression, Utils.ExpressionConstantTrue);

            return allExpression;
        }

        public override SelectExpression HandleNode(SelectExpression select, Expression predicate, MethodInfo method)
        {
            var parameter = predicate.GetParameters().FirstOrDefault();

            Utils.CheckNull(select, "query");

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            predicate = HandlePredicate(select, predicate).UnWrap();

            select.Projections.Clear();
            select.AddProjection(Utils.ExpressionConstantTrue);
            select.CombineCondtion(predicate.NodeType == ExpressionType.Lambda
                ? Expression.Not(predicate.As<LambdaExpression>().Body)
                : Expression.Not(predicate));
            select.QueryModel = new QueryEntityModel(select, predicate, typeof(bool), select.QueryModel);

            return select;
        }

        protected Expression HandlePredicate(SelectExpression select, Expression predicate)
        {
            predicate = new RelationExpressionVisitor(QueryContext).Compile(predicate);
            predicate = new RelationExpressionConvertVisitor().Visit(predicate);
            predicate = new QueryModelRewriterExpressionVisitor(select, QueryContext).ChangeQueryModel(predicate);

            return predicate;
        }
    }
}
