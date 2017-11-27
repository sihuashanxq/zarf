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

        public GroupByTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments.FirstOrDefault());
            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            MapQuerySource(GetFirstLambdaParameter(methodCall.Arguments.LastOrDefault()), query);
            query.Groups.Add(
                new GroupExpression(
                    GetColumns(
                        GetCompiledExpression(
                            methodCall.Arguments.LastOrDefault()
                        )
                    ).Select(item => item.Expression).OfType<ColumnExpression>()
                )
            );

            return query;
        }
    }
}
