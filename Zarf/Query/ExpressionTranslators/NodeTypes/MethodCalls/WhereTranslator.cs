using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class WhereTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static WhereTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Where");
        }

        public WhereTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            if (query.Where != null && (query.Columns.Count != 0 || query.Sets.Count != 0))
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[1]), query);

            var predicate = GetCompiledExpression(methodCall.Arguments[1]).UnWrap();
            if (true)
            {
                var exists = new ExistsExpression(predicate.As<LambdaExpression>().Body.As<BinaryExpression>().Left.As<QueryExpression>());
                exists.Query.Columns.Add(new Mapping.ColumnDescriptor() { Expression = Expression.Constant(1) });
                query.AddWhere(exists);
            }
            else
            {
                query.AddWhere(predicate);
            }

            if (methodCall.Method.Name == "SingleOrDefault")
            {
                query.DefaultIfEmpty = true;
            }

            return query;
        }
    }
}