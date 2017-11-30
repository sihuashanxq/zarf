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

        public AllTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            if (query.Where != null && (query.Projections.Count != 0 || query.Sets.Count != 0))
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);
            }
         
            RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[1]), query);

            var key = GetCompiledExpression(methodCall.Arguments[1].UnWrap()).UnWrap();
            var col = new ColumnDescriptor(Utils.ExpressionOne);

            if (key.NodeType == ExpressionType.Lambda)
                query.AddWhere(Expression.Not(key.As<LambdaExpression>().Body));
            else
                query.AddWhere(Expression.Not(key));

            query.Projections.Clear();
            query.Projections.Add(col);
            return new AllExpression(query);
        }
    }
}
