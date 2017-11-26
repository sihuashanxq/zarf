using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Zarf.Extensions;
using Zarf.Mapping;
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

        public AllTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate( MethodCallExpression methodCall)
        {
            var query = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var selector = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            MapQuerySource(selector.Parameters.LastOrDefault(), query);

            if (query.Where != null && (query.Projections.Count != 0 || query.Sets.Count != 0))
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }

            var keySelector = Compiler.Compile(methodCall.Arguments[1].UnWrap()).UnWrap();

            query.Projections.Clear();
            query.Projections.Add(new ColumnDescriptor() { Expression = Expression.Constant(1) });
            query.AddWhere(Expression.Not(keySelector.As<LambdaExpression>()?.Body ?? keySelector));

            return new AllExpression(query);
        }
    }
}
