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
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Skip");
        }

        public SkipTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate( MethodCallExpression methodCall)
        {
            var query = Compiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var offset = methodCall.Arguments[1].As<ConstantExpression>().Value;

            query.Offset = new SkipExpression(Convert.ToInt32(offset), query.Orders.ToList());

            if (query.Projections.Count == 0)
            {
                query.Projections.AddRange(Context.ProjectionScanner.Scan(query));
            }

            query.Projections.Add(new ColumnDescriptor() { Expression = query.Offset });
            query.Orders.Clear();
            query = query.PushDownSubQuery(Context.Alias.GetNewTable(), Context.UpdateRefrenceSource);

            var column = new ColumnExpression(query, new Column("__rowIndex__"), typeof(int));
            var predicate = Expression.MakeBinary(ExpressionType.GreaterThan, column, Expression.Constant(offset));
            var lambda = Expression.Lambda(predicate);
            query.AddWhere(lambda);

            return query;
        }
    }
}
