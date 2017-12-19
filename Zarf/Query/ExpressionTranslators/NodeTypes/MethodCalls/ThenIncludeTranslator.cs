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
            SupprotedMethods = new[] { ReflectionUtil.ThenInclude };
        }

        public ThenIncludeTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var rootQuery = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var propertyPath = methodCall.Arguments[1].UnWrap().As<LambdaExpression>().Body.As<MemberExpression>();
            var previousNaviation = Context.PropertyNavigationContext.GetLastNavigation();

            if (propertyPath == null || previousNaviation == null)
            {
                throw new ArgumentException("need Include before ThenInclude!");
            }

            var previousQuery = previousNaviation.RefrenceQuery;
            var propertyEleType = propertyPath.Member.GetPropertyType().GetCollectionElementType();
            var innerQuery = new QueryExpression(propertyEleType, Context.ColumnCaching, Context.Alias.GetNewTable());

            //关联关系
            var lambda = methodCall.Arguments[2].UnWrap().As<LambdaExpression>();
            MapParameterWithQuery(GetFirstParameter(lambda), previousQuery);

            var relation = Compiler.Compile(lambda);
            MapParameterWithQuery(GetLastParameter(lambda), innerQuery);
            var condtion = Compiler.Compile(lambda);

            innerQuery.AddJoin(new JoinExpression(previousQuery, condtion));
            //innerQuery.AddColumns(Context.ProjectionScanner.Scan(innerQuery));

            //Context.PropertyNavigationContext.AddPropertyNavigation(
            //    propertyPath.Member,
            //    new PropertyNavigation(
            //         propertyPath.Member,
            //         innerQuery,
            //         Context.ProjectionScanner.Scan(relation).Select(item => item.Expression).ToList(),
            //         relation
            //    )
            //);

            return rootQuery;
        }
    }
}
