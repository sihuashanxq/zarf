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
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Take");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var limit = methodCall.Arguments[1].As<ConstantExpression>().Value;

            query.Limit = Convert.ToInt32(limit);
            return query;
        }
    }
}
