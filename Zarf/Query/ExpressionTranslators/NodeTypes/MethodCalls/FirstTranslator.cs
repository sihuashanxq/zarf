using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class FirstTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static FirstTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "First" || item.Name == "FirstOrDefault");
        }

        public FirstTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments.FirstOrDefault());

            if (methodCall.Arguments.Count == 2)
            {
                if (query.Sets.Count != 0)
                {
                    query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
                    query.Result = query.SubQuery.Result;
                }

                MapQuerySource(GetFirstLambdaParameter(methodCall.Arguments.LastOrDefault()), query);
                query.AddWhere(GetCompiledExpression(methodCall.Arguments.LastOrDefault()).UnWrap());
            }

            query.DefaultIfEmpty = methodCall.Method.Name == "FirstOrDefault";
            query.Limit = 1;
            return query;
        }
    }
}
