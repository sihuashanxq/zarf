using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AnyTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AnyTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Any");
        }

        public AnyTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var select = Compile<SelectExpression>(methodCall.Arguments[0]);
            var anyExpression = new AnyExpression(Translate(select, methodCall.Arguments[1], methodCall.Method));

            QueryContext.ExpressionMapper.Map(anyExpression, Utils.ExpressionConstantTrue);

            return anyExpression;
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

            select.Projections.Clear();
            select.AddProjection(Utils.ExpressionConstantTrue);
            select.CombineCondtion(predicate);

            select.QueryModel = new QueryEntityModel(select, predicate, typeof(bool), select.QueryModel);

            return select;
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(SelectExpression select)
        {
            return new RelationExpressionCompiler(QueryContext);
        }
    }
}
