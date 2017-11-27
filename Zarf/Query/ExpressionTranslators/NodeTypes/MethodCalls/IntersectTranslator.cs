using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class IntersectTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static IntersectTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Intersect");
        }

        public IntersectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments.FirstOrDefault());
            var setsQuery = GetCompiledExpression<QueryExpression>(methodCall.Arguments.LastOrDefault());

            Utils.CheckNull(query, "Query Expression");
            Utils.CheckNull(setsQuery, "Intersect Query Expression");

            if (setsQuery.Projections.Count == 0)
            {
                setsQuery.Projections.AddRange(GetColumns(setsQuery));
            }

            query.Sets.Add(new IntersectExpression(setsQuery));
            return query;
        }
    }
}
