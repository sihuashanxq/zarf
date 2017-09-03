using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class GroupByTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static GroupByTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "GroupBy");
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var keySelector = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(context.AliasGenerator.GetNewTableAlias(), context.UpdateRefrenceSource);
            }

            context.QuerySourceProvider.AddSource(keySelector.Parameters.First(), query);
            var selector = transformVisitor.Visit(keySelector);

            query.Groups.Add(
                new GroupExpression(
                    context.ProjectionFinder.Find<ColumnExpression>(selector))
            );

            return query;
        }
    }
}
