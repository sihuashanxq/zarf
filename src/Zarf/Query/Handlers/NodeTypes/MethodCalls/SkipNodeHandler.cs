using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class SkipNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SkipNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Skip");
        }

        public SkipNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression HandleNode(
            SelectExpression select,
            Expression offSet, 
            MethodInfo method)
        {
            select.Offset = new SkipExpression((int)offSet.As<ConstantExpression>().Value, select.Orders.ToList());
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
