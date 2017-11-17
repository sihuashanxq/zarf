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
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Select");
        }

        public override Expression Translate(IQueryContext queryContext, MethodCallExpression methodCall, IQueryCompiler queryCompiler)
        {
            var query = queryCompiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var selector = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(queryContext.Alias.GetNewTable(), queryContext.UpdateRefrenceSource);
            }

            queryContext.QuerySourceProvider.AddSource(selector.Parameters.FirstOrDefault(), query);

            var entityNew = queryCompiler.Compile(selector).UnWrap();

            query.Projections.AddRange(queryContext.ProjectionScanner.Scan(entityNew));
            query.Result = new EntityResult(entityNew, methodCall.Method.ReturnType.GetCollectionElementType());
            return query;
        }
    }
}