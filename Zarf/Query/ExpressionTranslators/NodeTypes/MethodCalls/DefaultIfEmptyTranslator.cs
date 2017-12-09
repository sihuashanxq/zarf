using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Reflection;
using System.Linq;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    internal class DefaultIfEmptyTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static DefaultIfEmptyTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "DefaultIfEmpty");
        }

        public DefaultIfEmptyTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            query.DefaultIfEmpty = true;
            return query;
        }
    }
}
