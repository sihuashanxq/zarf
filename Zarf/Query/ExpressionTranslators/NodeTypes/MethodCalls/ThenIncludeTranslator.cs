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

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor tranformVisitor)
        {
            var rootQuery = tranformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var propertyPath = methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body.As<MemberExpression>();
            var previousNaviation = context.PropertyNavigationContext.GetLastNavigation();

            if (propertyPath == null || previousNaviation == null)
            {
                throw new ArgumentException("need Include before ThenInclude!");
            }

            var previousQuery = previousNaviation.RefrenceQuery;
            var propertyEleType = propertyPath.Member.GetMemberInfoType().GetElementTypeInfo();
            var innerQuery = new QueryExpression(propertyEleType, context.Alias.GetNewTable());

            //关联关系
            var lambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();

            context.QuerySourceProvider.AddSource(lambda.Parameters[0], previousQuery);

            var relation = tranformVisitor.Visit(lambda);

            context.QuerySourceProvider.AddSource(lambda.Parameters[1], innerQuery);
            var condtion = tranformVisitor.Visit(lambda);

            innerQuery.AddJoin(new JoinExpression(previousQuery, condtion));
            innerQuery.Projections.AddRange(innerQuery.GenerateColumns());

            context.PropertyNavigationContext.AddPropertyNavigation(
                propertyPath.Member,
                new PropertyNavigation(
                     propertyPath.Member,
                     innerQuery,
                     context.ProjectionFinder.Find(relation),
                     relation
                )
            );

            return rootQuery;
        }
    }
}
