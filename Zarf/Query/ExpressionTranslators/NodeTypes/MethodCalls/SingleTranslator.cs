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

        public SingleTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate( MethodCallExpression methodCall)
        {
            var query = new WhereTranslator(Context, Compiler).Translate(methodCall) as QueryExpression;
            query.DefaultIfEmpty = methodCall.Method.Name == "SingleOrDefault";
            query.Limit = 2;
            return query;
        }
    }
}
