using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Queries.Expressions;
using Zarf.Queries.ExpressionVisitors;

namespace Zarf.Queries.ExpressionTranslators.NodeTypes
{
    public class ToListTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static ToListTranslator()
        {
            SupprotedMethods = typeof(IQuery<>).GetMethods().Where(item => item.Name == "ToList");
        }

        public ToListTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var obj = methodCall.Object ?? methodCall.Arguments[0];

            if (!typeof(IQueryable).IsAssignableFrom(obj.Type) &&
                !typeof(IQuery).IsAssignableFrom(obj.Type))
            {
                return methodCall;
            }

            var compildNode = GetCompiledExpression(obj);
            if (compildNode is QueryExpression query)
            {
                query.QueryModel.ModelType = methodCall.Method.ReturnType;
            }

            return compildNode;
        }
    }
}
