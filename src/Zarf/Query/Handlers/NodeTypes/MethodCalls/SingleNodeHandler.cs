using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class SingleNodeHandler : WhereNodeHandler
    {
        public new static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SingleNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Single" || item.Name == "SingleOrDefault");
        }

        public SingleNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            var select = base.HandleNode(methodCall).As<SelectExpression>();

            Utils.CheckNull(select, "query");

            if (select.QueryModel.RefrencedOuterColumns.Count == 0)
            {
                select.Limit = 2;
            }

            return select;
        }
    }
}
