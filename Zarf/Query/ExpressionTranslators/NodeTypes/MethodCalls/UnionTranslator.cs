using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class UnionTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static UnionTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Union" || item.Name == "Concat");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            if (methodCall.Arguments.Count != 2)
            {
                throw new NotImplementedException("not supproted!");
            }

            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var setsQuery = transformVisitor.Visit(methodCall.Arguments[1]).As<QueryExpression>();

            Utils.CheckNull(query, "Query Expression");
            Utils.CheckNull(setsQuery, "Union Query Expression");

            query.Sets.Add(new UnionExpression(setsQuery));
            //UNION 默认比较 Concat 不比较
            if (methodCall.Method.Name == "Union")
            {
                if (query.Projections.Count == 0)
                {
                    query.Projections.AddRange(query.GenerateColumns());
                }

                query = query.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
                query.IsDistinct = true;
            }

            if (setsQuery.Projections.Count == 0)
            {
                var entityType = new Mapping.EntityTypeDescriptorFactory().Create(setsQuery.Type);
                foreach (var item in entityType.GetWriteableMembers())
                {
                    setsQuery.Projections.Add(new ColumnExpression(setsQuery, item, item.Name));
                }
            }

            return query;
        }
    }
}
