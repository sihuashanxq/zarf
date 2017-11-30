using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class JoinTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static JoinTranslator()
        {
            SupprotedMethods = new[] { ReflectionUtil.Join };
        }

        public JoinTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var queries = new List<QueryExpression>() { query };
            var joins = methodCall.Arguments[1].As<ConstantExpression>()?.Value as List<JoinQuery>;
            if (joins == null)
            {
                throw new Exception("error join query!");
            }

            foreach (var item in joins)
            {
                queries.Add(GetJoinQuery(item.InternalDbQuery));
                for (var i = 0; i < item.Predicate.Parameters.Count; i++)
                {
                    RegisterQuerySource(item.Predicate.Parameters[i], queries[i]);
                }

                query.AddJoin(new JoinExpression(queries.Last(), GetCompiledExpression(item.Predicate), item.JoinType));
            }

            return query;
        }

        private QueryExpression GetJoinQuery(IInternalDbQuery dbQuery)
        {
            var query = dbQuery.GetType().GetProperty("Expression").GetValue(dbQuery) as Expression;
            if (query == null)
            {
                throw new Exception("error join query!");
            }

            return GetCompiledExpression<QueryExpression>(query);
        }
    }
}
