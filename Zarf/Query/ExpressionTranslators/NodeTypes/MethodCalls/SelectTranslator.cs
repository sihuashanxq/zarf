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

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var rootQuery = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var selector = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (rootQuery.Sets.Count != 0)
            {
                rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
            }

            context.QuerySourceProvider.AddSource(selector.Parameters.FirstOrDefault(), rootQuery);

            var entityNew = transformVisitor.Visit(selector).UnWrap();
            var projections = context.ProjectionScanner.Scan(entityNew);

            rootQuery.Projections.AddRange(projections.Select(item => item.Expression));
            rootQuery.Result = new EntityResult(entityNew, methodCall.Method.ReturnType.GetCollectionElementType());
            return rootQuery;
        }
    }
}