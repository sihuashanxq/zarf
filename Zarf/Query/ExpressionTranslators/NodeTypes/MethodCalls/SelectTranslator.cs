using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    /// <summary>
    /// Select Query
    /// </summary>
    public class SelectTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SelectTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Select")
                .Concat(new[] { ReflectionUtil.Select });
        }

        public SelectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var methodBody = methodCall.Method.GetGenericMethodDefinition();
            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            if (methodBody == ReflectionUtil.Select)
            {
                RegisterJoinSelectQueries(query, methodCall.Arguments[1]);
            }
            else
            {
                RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[1]), query);
            }

            var template = GetCompiledExpression(methodCall.Arguments[1]).UnWrap();
            query.Projections.AddRange(GetColumns(template));
            query.Result = new EntityResult(template, methodCall.Method.ReturnType.GetCollectionElementType());
            return query;
        }

        protected virtual void RegisterJoinSelectQueries(QueryExpression query, Expression selector)
        {
            var parameters = GetLambdaParameteres(selector);
            var i = 0;
            while (i < parameters.Count)
            {
                var parameter = parameters[i];
                if (i == 0)
                {
                    RegisterQuerySource(parameter, query);
                }
                else
                {
                    RegisterQuerySource(parameter, query.Joins[i - 1].Table.As<QueryExpression>());
                }
                i++;
            }
        }
    }
}