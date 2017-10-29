using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class SingleTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SingleTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Single" || item.Name == "SingleOrDefault");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, IQueryCompiler queryCompiler)
        {
            var query = new WhereTranslator().Translate(context, methodCall, queryCompiler) as QueryExpression;

            if (methodCall.Method.Name == "SingleOrDefault")
            {
                query.DefaultIfEmpty = true;
            }

            query.Limit = 2;
            return query;
        }
    }
}
