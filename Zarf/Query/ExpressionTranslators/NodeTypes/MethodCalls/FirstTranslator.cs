using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class FirstTranslator : WhereTranslator
    {
        public new static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static FirstTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "First" || item.Name == "FirstOrDefault");
        }

        public FirstTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = base.Translate(methodCall).As<QueryExpression>();

            Utils.CheckNull(query, "query");

            if (query.QueryModel.RefrencedColumns.Count == 0)
            {
                //else 内存过滤
                query.Limit = 1;
            }

            query.QueryModel.ModelType = methodCall.Method.ReturnType;

            return query;
        }
    }
}
