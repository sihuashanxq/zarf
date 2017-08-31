using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    internal class DistinctTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static DistinctTranslator()
        {
            SupprotedMethods= ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Distinct");
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            if (methodCall.Arguments.Count != 1)
            {
                throw new NotImplementedException("Distinct method not supported arguments!");
            }

            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            query.IsDistinct = true;
            return query;
        }
    }
}
