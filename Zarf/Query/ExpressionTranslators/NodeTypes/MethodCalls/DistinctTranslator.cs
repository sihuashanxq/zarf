using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries.ExpressionTranslators.Methods
{
    internal class DistinctTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static DistinctTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Distinct");
        }

        public DistinctTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);

            Utils.CheckNull(query, "query");

            return Translate(query, null);
        }

        public virtual QueryExpression Translate(QueryExpression query, Expression exp)
        {
            query.IsDistinct = true;

            return query;
        }
    }
}
