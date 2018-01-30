using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;
using Zarf.Query.Handlers.NodeTypes.MethodCalls;

namespace Zarf.Query.Handlers.Methods
{
    public class UnionNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static UnionNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Union" || item.Name == "Concat");
        }

        public UnionNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }
  
        public override SelectExpression HandleNode(SelectExpression select, Expression sets,MethodInfo method)
        {
            var setsSelect = Compile<SelectExpression>(sets);

            Utils.CheckNull(select, "Query Expression");
            Utils.CheckNull(setsSelect, "Except Query Expression");

            select.Sets.Add(new UnionExpression(setsSelect));
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
