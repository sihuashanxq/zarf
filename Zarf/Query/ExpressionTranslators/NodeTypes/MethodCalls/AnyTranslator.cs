using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Mapping;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AnyTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AnyTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Any");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, IQueryCompiler queryCompiler)
        {
            var rootQuery = queryCompiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var lambda = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (rootQuery.Where != null && (rootQuery.Projections.Count != 0 || rootQuery.Sets.Count != 0))
            {
                rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
            }

            rootQuery.Projections.Clear();
            rootQuery.Projections.Add(new Projection() { Expression = Expression.Constant(1) });

            context.QuerySourceProvider.AddSource(lambda.Parameters.First(), rootQuery);

            var condition = queryCompiler.Compile(lambda).UnWrap();
            rootQuery.AddWhere(condition);

            return new AnyExpression(rootQuery);
        }
    }
}
