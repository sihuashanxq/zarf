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
            SupprotedMethods = new[] { DataQueryable.IncludeMethodInfo };
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor tranformVisitor)
        {
            var rootQuery = tranformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var propertyPath = methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body.As<MemberExpression>();

            if (propertyPath == null)
            {
                throw new ArgumentException("item=>item.Property");
            }

            var propertyElementType = propertyPath.Member.GetMemberInfoType().GetElementTypeInfo();
            var innerQuery = new QueryExpression(propertyElementType, context.AliasGenerator.GetNewTableAlias());

            var conditionLambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();

            context.QuerySourceProvider.AddSource(conditionLambda.Parameters[0], rootQuery);

            var filter = tranformVisitor.Visit(conditionLambda);

            context.IncludesCondtion[propertyPath.Member] = filter;
            context.IncludesCondtionParameter[propertyPath.Member] = context.ProjectionFinder.FindProjections(filter);

            context.QuerySourceProvider.AddSource(conditionLambda.Parameters[1], innerQuery);
            var condtion = tranformVisitor.Visit(conditionLambda);

            innerQuery.AddJoin(new JoinExpression(rootQuery, condtion));
            innerQuery.Projections.AddRange(innerQuery.GenerateColumns());

            context.Includes[propertyPath.Member] = innerQuery;

            return rootQuery;
        }
    }
}
