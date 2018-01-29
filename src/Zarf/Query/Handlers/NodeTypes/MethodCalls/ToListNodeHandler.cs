using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core.Internals;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.Visitors;

namespace Zarf.Query.Handlers.NodeTypes
{
    public class ToListNodeHandler : QueryNodeHandler<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static ToListNodeHandler()
        {
            SupprotedMethods = typeof(IQuery<>).GetMethods().Where(item => item.Name == "ToList" || item.Name == "AsEnumerable");
        }

        public ToListNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            var obj = methodCall.Object ?? methodCall.Arguments[0];
            if (obj.NodeType == ExpressionType.Extension)
            {
                return obj;
            }

            if (!typeof(IQueryable).IsAssignableFrom(obj.Type) &&
                !typeof(IQuery).IsAssignableFrom(obj.Type))
            {
                return methodCall;
            }

            var compildNode = Compile(obj);
            if (compildNode is SelectExpression select)
            {
                select.QueryModel.ModelType = methodCall.Method.ReturnType;
            }

            return compildNode;
        }
    }
}
