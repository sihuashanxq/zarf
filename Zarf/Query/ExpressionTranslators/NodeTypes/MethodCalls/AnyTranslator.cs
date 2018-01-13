using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Mapping;
using Zarf.Entities;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AnyTranslator : Translator<MethodCallExpression>
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
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var anyExpression = Translate(query, methodCall.Arguments[1]);

            Context.ExpressionMapper.Map(anyExpression, Utils.ExpressionConstantTrue);

            return anyExpression;
        }

        public virtual Expression Translate(QueryExpression query, Expression predicate)
        {
            var parameter = predicate.GetParameters().FirstOrDefault();

            Utils.CheckNull(query, "query");

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            predicate = CreateRealtionCompiler(query).Compile(predicate);
            predicate = new RelationExpressionVisitor().Visit(predicate);
            predicate = new SubQueryModelRewriter(query, Context).ChangeQueryModel(predicate);

            query.Projections.Clear();
            query.AddProjection(Utils.ExpressionConstantTrue);
            query.CombineCondtion(predicate);

            query.QueryModel = new QueryEntityModel(query, predicate, typeof(bool), query.QueryModel);

            return new AnyExpression(query);
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(QueryExpression query)
        {
            return new RelationExpressionCompiler(Context);
        }
    }
}
