using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class SingleTranslator : WhereTranslator
    {
        public new static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SingleTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Single" || item.Name == "SingleOrDefault");
        }

        public SingleTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = base.Translate(methodCall).As<QueryExpression>();

            Utils.CheckNull(query, "query");

            query.Limit = 2;

            return query;
        }
    }
}
