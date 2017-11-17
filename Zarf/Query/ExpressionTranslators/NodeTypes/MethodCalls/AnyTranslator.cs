﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Mapping;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AnyTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AnyTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Any");
        }

        public override Expression Translate(IQueryContext queryContext, MethodCallExpression methodCall, IQueryCompiler queryCompiler)
        {
            var query = queryCompiler.Compile(methodCall.Arguments[0]).As<QueryExpression>();
            var selector = queryCompiler.Compile(methodCall.Arguments[1].UnWrap()).UnWrap();

            if (query.Where != null && (query.Projections.Count != 0 || query.Sets.Count != 0))
            {
                query = query.PushDownSubQuery(queryContext.Alias.GetNewTable(), queryContext.UpdateRefrenceSource);
            }

            query.Projections.Clear();
            query.Projections.Add(new Projection() { Expression = Expression.Constant(1) });
            query.AddWhere(selector);

            return new AnyExpression(query);
        }
    }
}
