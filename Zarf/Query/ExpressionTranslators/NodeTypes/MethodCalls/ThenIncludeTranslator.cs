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

            var previousQuery = context
                .Includes
                .LastOrDefault()
                .Value;

            if (propertyPath == null || previousQuery == null)
            {
                throw new ArgumentException("need Include before ThenInclude!");
            }

            var propertyElementType = propertyPath.Member.GetMemberInfoType().GetElementTypeInfo();
            var innerQuery = new QueryExpression(propertyElementType, context.CreateAlias());

            var conditionLambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();

            context.QuerySource[conditionLambda.Parameters[0]] = previousQuery;

            context.Projections.Clear();
            var filter = tranformVisitor.Visit(conditionLambda);

            context.IncludesCondtion[propertyPath.Member] = filter;
            context.IncludesCondtionParameter[propertyPath.Member] = context.Projections.ToList();

            context.QuerySource[conditionLambda.Parameters[1]] = innerQuery;
            var condtion = tranformVisitor.Visit(conditionLambda);

            innerQuery.AddJoin(new JoinExpression(previousQuery, condtion));
            innerQuery.Projections.AddRange(innerQuery.GenerateColumns());

            context.Includes[propertyPath.Member] = innerQuery;

            return rootQuery;
        }
    }
}
