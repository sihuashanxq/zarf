using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;
using Zarf.Query.Internals;

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

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            predicate = HandlePredicate(select, predicate);

            select.Projections.Clear();
            select.AddProjection(Utils.ExpressionConstantTrue);
            select.CombineCondtion(predicate);
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
