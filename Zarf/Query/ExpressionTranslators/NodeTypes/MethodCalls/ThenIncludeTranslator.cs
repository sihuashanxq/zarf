using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class ThenIncludeTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static ThenIncludeTranslator()
        {
            SupprotedMethods = new[] { DataQueryable.ThenIncludeMethodInfo };
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, IQueryCompiler queryCompiler)
        {
            var rootQuery = queryCompiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var propertyPath = methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body.As<MemberExpression>();
            var previousNaviation = context.PropertyNavigationContext.GetLastNavigation();

            if (propertyPath == null || previousNaviation == null)
            {
                throw new ArgumentException("need Include before ThenInclude!");
            }

            var previousQuery = previousNaviation.RefrenceQuery;
            var propertyEleType = propertyPath.Member.GetMemberTypeInfo().GetCollectionElementType();
            var innerQuery = new QueryExpression(propertyEleType, context.Alias.GetNewTable());

            //关联关系
            var lambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();

            context.QuerySourceProvider.AddSource(lambda.Parameters[0], previousQuery);

            var relation = queryCompiler.Compile(lambda);

            context.QuerySourceProvider.AddSource(lambda.Parameters[1], innerQuery);
            var condtion = queryCompiler.Compile(lambda);

            innerQuery.AddJoin(new JoinExpression(previousQuery, condtion));
            innerQuery.Projections.AddRange(context.ProjectionScanner.Scan(innerQuery));

            context.PropertyNavigationContext.AddPropertyNavigation(
                propertyPath.Member,
                new PropertyNavigation(
                     propertyPath.Member,
                     innerQuery,
                     context.ProjectionScanner.Scan(relation).Select(item => item.Expression).ToList(),
                     relation
                )
            );

            return rootQuery;
        }
    }
}
