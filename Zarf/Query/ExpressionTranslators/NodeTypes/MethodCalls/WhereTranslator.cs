using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping;
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
            var query = GetQueryExpression(methodCall.Arguments[0]);
            MapParameterWithQuery(GetFirstParameter(methodCall.Arguments[1]), query);
            HandleQueryCondtion(query, methodCall.Arguments[1]);

            if (methodCall.Method.Name == "SingleOrDefault")
            {
                query.DefaultIfEmpty = true;
            }

            return query;
        }

        private QueryExpression GetQueryExpression(Expression exp)
        {
            var query = GetCompiledExpression<QueryExpression>(exp);
            if (query.Where != null && (query.Projections.Count != 0 || query.Sets.Count != 0))
            {
                return query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            return query;
        }

        private void HandleQueryCondtion(QueryExpression query, Expression condtion)
        {
            condtion = GetCompiledExpression(condtion);
            condtion = HandleCondtion(condtion);
            query.CombineCondtion(condtion);
        }
    }
}