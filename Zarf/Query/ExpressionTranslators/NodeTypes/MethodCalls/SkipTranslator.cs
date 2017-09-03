using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Linq;
using System.Reflection;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class SkipTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SkipTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Skip");
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var offset = methodCall.Arguments[1].As<ConstantExpression>().Value;

            query.Offset = new SkipExpression(Convert.ToInt32(offset), query.Orders.ToList());

            if (query.Projections.Count == 0)
            {
                query.Projections.AddRange(query.GenerateColumns());
            }

            query.Projections.Add(query.Offset);
            query.Orders.Clear();
            query = query.PushDownSubQuery(context.AliasGenerator.GetNewTableAlias(), context.UpdateRefrenceSource);

            var column = new ColumnExpression(query, new Column("__rowIndex__"), typeof(int));
            var predicate = Expression.MakeBinary(ExpressionType.GreaterThan, column, Expression.Constant(offset));
            var lambda = Expression.Lambda(predicate);
            query.AddWhere(lambda);

            return query;
        }
    }
}
