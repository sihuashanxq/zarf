using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class IncludeTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static IncludeTranslator()
        {
            SupprotedMethods = new[] { DataQueryable.IncludeMethodInfo};
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor tranformVisitor)
        {
            var rootQuery = tranformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var propertyPath = methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body.As<MemberExpression>();

            if (propertyPath == null)
            {
                throw new ArgumentException("item=>item.Property");
            }

            var propertyEleType = propertyPath.Member.GetMemberInfoType().GetCollectionElementType();
            var innerQuery = new QueryExpression(propertyEleType, context.Alias.GetNewTable());

            //关联关系
            var lambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();

            context.QuerySourceProvider.AddSource(lambda.Parameters[0], rootQuery);

            var relation = tranformVisitor.Visit(lambda);

            context.QuerySourceProvider.AddSource(lambda.Parameters[1], innerQuery);
            var condtion = tranformVisitor.Visit(lambda);

            innerQuery.AddJoin(new JoinExpression(rootQuery, condtion));
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
