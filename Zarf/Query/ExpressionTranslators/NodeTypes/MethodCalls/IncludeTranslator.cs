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
            SupprotedMethods = new[] { ReflectionUtil.Include };
        }

        public IncludeTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate( MethodCallExpression methodCall)
        {
            var rootQuery = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var propertyPath = methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body.As<MemberExpression>();

            if (propertyPath == null)
            {
                throw new ArgumentException("item=>item.Property");
            }

            var propertyEleType = propertyPath.Member.GetPropertyType().GetCollectionElementType();
            var innerQuery = new QueryExpression(propertyEleType, Context.Alias.GetNewTable());

            //关联关系
            var lambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();
            MapQuerySource( lambda.Parameters.FirstOrDefault(), rootQuery);
            var relation = Compiler.Compile(lambda);

            MapQuerySource( lambda.Parameters.LastOrDefault(), innerQuery);
            var condtion = Compiler.Compile(lambda);

            innerQuery.AddJoin(new JoinExpression(rootQuery, condtion));
            innerQuery.Projections.AddRange(Context.ProjectionScanner.Scan(innerQuery));

            Context.PropertyNavigationContext.AddPropertyNavigation(
                propertyPath.Member,
                new PropertyNavigation(
                     propertyPath.Member,
                     innerQuery,
                     Context.ProjectionScanner.Scan(relation).Select(item => item.Expression).ToList(),
                     lambda
                )
            );

            return rootQuery;
        }
    }
}
