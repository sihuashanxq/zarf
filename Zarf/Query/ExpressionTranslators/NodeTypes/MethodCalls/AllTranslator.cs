using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AllTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AllTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "All");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor tranformVisitor)
        {
            var rootQuery = tranformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var lambda = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (rootQuery.Where != null && (rootQuery.Projections.Count != 0 || rootQuery.Sets.Count != 0))
            {
                rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
            }

            rootQuery.Projections.Clear();
            rootQuery.Projections.Add(new ExpressionVisitors.Projection() { Expression = Expression.Constant(1) });

            context.QuerySourceProvider.AddSource(lambda.Parameters.First(), rootQuery);
            var condition = tranformVisitor.Visit(lambda).UnWrap();
            if (condition.Is<LambdaExpression>())
            {
                rootQuery.AddWhere(Expression.Not(condition.As<LambdaExpression>().Body));
            }
            else
            {
                rootQuery.AddWhere(Expression.Not(condition));
            }

            return new AllExpression(rootQuery);
        }
    }
}
