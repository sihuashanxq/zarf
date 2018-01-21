using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
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
            var select = base.Translate(methodCall).As<SelectExpression>();

            Utils.CheckNull(select, "query");

            if (select.QueryModel.RefrencedColumns.Count == 0)
            {
                select.Limit = 2;
            }

            return select;
        }
    }
}
