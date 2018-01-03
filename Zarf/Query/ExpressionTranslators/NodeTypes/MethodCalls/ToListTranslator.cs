using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class ToListTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static ToListTranslator()
        {
            SupprotedMethods = typeof(Enumerable).GetMethods().Where(item => item.Name == "ToList");
        }

        public ToListTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            if (!typeof(IQueryable).IsAssignableFrom(methodCall.Arguments[0].Type))
            {
                return methodCall;
            }

            var compildNode = GetCompiledExpression(methodCall.Arguments[0]);
            if (compildNode is QueryExpression query)
            {
                query.QueryModel.ModeType = methodCall.Method.ReturnType;
            }

            return compildNode;
        }
    }
}
