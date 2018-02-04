using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class IntersectNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static IntersectNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Intersect");
        }

        public IntersectNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression HandleNode(SelectExpression select, Expression sets, MethodInfo method)
        {
            var setsSelect = Compile<SelectExpression>(sets);

            Utils.CheckNull(select, "Query Expression");
            Utils.CheckNull(setsSelect, "Except Query Expression");

            select.Sets.Add(new IntersectExpression(setsSelect));
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
