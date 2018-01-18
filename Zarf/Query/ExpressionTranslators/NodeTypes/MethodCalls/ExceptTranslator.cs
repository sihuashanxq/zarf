using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class ExceptTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static ExceptTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Except");
        }

        public ExceptTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);

            return Translate(query, methodCall.Arguments[1]);
        }

        public virtual QueryExpression Translate(QueryExpression query, Expression sets)
        {
            var setsQuery = GetCompiledExpression<QueryExpression>(sets);

            Utils.CheckNull(query, "Query Expression");
            Utils.CheckNull(setsQuery, "Except Query Expression");

            query.Sets.Add(new ExceptExpression(setsQuery));

            return query;
        }
    }
}
