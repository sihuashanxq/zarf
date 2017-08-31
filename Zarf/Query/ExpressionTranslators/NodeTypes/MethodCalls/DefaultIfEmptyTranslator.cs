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
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "DefaultIfEmpty");
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            if (methodCall.Arguments.Count != 1)
            {
                throw new NotImplementedException("Distinct method not supported arguments!");
            }

            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            query.DefaultIfEmpty = true;
            return query;
        }
    }
}
