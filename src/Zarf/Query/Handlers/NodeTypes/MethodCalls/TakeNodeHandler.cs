using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class TakeNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static TakeNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Take");
        }

        public TakeNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression HandleNode(SelectExpression select, Expression limit, MethodInfo method)
        {
            select.Limit = Convert.ToInt32(limit.As<ConstantExpression>().Value);
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
