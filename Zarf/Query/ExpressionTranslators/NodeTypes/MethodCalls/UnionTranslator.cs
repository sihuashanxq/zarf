using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class UnionTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static UnionTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Union" || item.Name == "Concat");
        }

        public UnionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var setsQuery = GetCompiledExpression<QueryExpression>(methodCall.Arguments[1]);

            Utils.CheckNull(query, "Query Expression");
            Utils.CheckNull(setsQuery, "Union Query Expression");

            query.Sets.Add(new UnionExpression(setsQuery));
            //UNION 默认比较 Concat 不比较
            if (methodCall.Method.Name == "Union")
            {
                if (query.Columns.Count == 0)
                {
                    query.AddColumns(GetColumns(query));
                }

                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
                query.IsDistinct = true;
            }

            if (setsQuery.Columns.Count == 0)
            {
                setsQuery.AddColumns(GetColumns(setsQuery));
            }

            return query;
        }
    }
}
