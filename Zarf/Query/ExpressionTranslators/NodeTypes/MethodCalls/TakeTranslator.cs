using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class TakeTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static TakeTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Take");
        }

        public TakeTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);

            query.Limit = Convert.ToInt32(methodCall.Arguments[1].As<ConstantExpression>().Value);
            return query;
        }
    }
}
