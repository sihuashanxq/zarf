using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class FirstNodeHandler : WhereNodeHandler
    {
        public new static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static FirstNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "First" || item.Name == "FirstOrDefault");
        }

        public FirstNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            var select = base.HandleNode(methodCall).As<SelectExpression>();

            Utils.CheckNull(select, "query");

            if (select.QueryModel.RefrencedColumns.Count == 0)
            {
                select.Limit = 1;
            }

            return select;
        }
    }
}
