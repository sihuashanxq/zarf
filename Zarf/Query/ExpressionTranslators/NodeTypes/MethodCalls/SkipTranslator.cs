using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Linq;
using System.Reflection;
using Zarf.Mapping;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class SkipTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SkipTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Skip");
        }

        public SkipTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var offset = (int)methodCall.Arguments[1].As<ConstantExpression>().Value;
            var skip = new SkipExpression(offset, query.Orders.ToList());

            query.AddProjection(skip);
            query = query.PushDownSubQuery(Context.Alias.GetNewTable());

            var skipColumn = new ColumnExpression(query, new Column("__rowIndex__"), typeof(int));
            var skipCondtion = Expression.MakeBinary(
                ExpressionType.GreaterThan, 
                skipColumn, 
                Expression.Constant(offset));

            query.CombineCondtion(Expression.Lambda(skipCondtion));

            return query;
        }
    }
}
