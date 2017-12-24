using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class WhereTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static WhereTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Where");
        }

        public WhereTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var predicate = methodCall.Arguments[1];
            var parameter = predicate.GetParameters().FirstOrDefault();

            Utils.CheckNull(query, "query");

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            predicate = CreateRealtionCompiler(query).Compile(predicate);
            predicate = new RelationExpressionVisitor().Visit(predicate);

            query.DefaultIfEmpty = methodCall.Method.Name.Contains("Default");
            query.CombineCondtion(predicate);

            var s = Context.DbContextParts.CommandTextBuilder.Build(query);

            return query;
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(QueryExpression query)
        {
            return new RelationExpressionCompiler(Context);
        }
    }
}