using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Handlers.Methods;
using Zarf.Query.Handlers.NodeTypes.MethodCalls;

namespace Zarf.Query.Handlers.NodeTypes
{
    public class MethodCallNodeHandler : QueryNodeHandler<MethodCallExpression>
    {
        protected readonly Dictionary<MethodInfo, IQueryNodeHandler> _handlers = new Dictionary<MethodInfo, IQueryNodeHandler>();

        public MethodCallNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiler) : base(queryContext, queryCompiler)
        {
            RegisterHandler(AggregateNodeHandler.SupprotedMethods, new AggregateNodeHandler(queryContext, queryCompiler));
            RegisterHandler(SelectNodeHandler.SupprotedMethods, new SelectNodeHandler(queryContext, queryCompiler));
            RegisterHandler(DistinctNodeHandler.SupprotedMethods, new DistinctNodeHandler(queryContext, queryCompiler));
            RegisterHandler(ExceptNodeHandler.SupprotedMethods, new ExceptNodeHandler(queryContext, queryCompiler));
            RegisterHandler(FirstNodeHandler.SupprotedMethods, new FirstNodeHandler(queryContext, queryCompiler));
            RegisterHandler(GroupByNodeHandler.SupprotedMethods, new GroupByNodeHandler(queryContext, queryCompiler));
            RegisterHandler(JoinNodeHandler.SupprotedMethods, new JoinNodeHandler(queryContext, queryCompiler));
            RegisterHandler(OrderByNodeHandler.SupprotedMethods, new OrderByNodeHandler(queryContext, queryCompiler));
            RegisterHandler(SingleNodeHandler.SupprotedMethods, new SingleNodeHandler(queryContext, queryCompiler));
            RegisterHandler(SkipNodeHandler.SupprotedMethods, new SkipNodeHandler(queryContext, queryCompiler));
            RegisterHandler(TakeNodeHandler.SupprotedMethods, new TakeNodeHandler(queryContext, queryCompiler));
            RegisterHandler(UnionNodeHandler.SupprotedMethods, new UnionNodeHandler(queryContext, queryCompiler));
            RegisterHandler(WhereNodeHandler.SupprotedMethods, new WhereNodeHandler(queryContext, queryCompiler));
            RegisterHandler(AllNodeHandler.SupprotedMethods, new AllNodeHandler(queryContext, queryCompiler));
            RegisterHandler(AnyNodeHandler.SupprotedMethods, new AnyNodeHandler(queryContext, queryCompiler));
            RegisterHandler(JoinSelectNodeHandler.SupprotedMethods, new JoinSelectNodeHandler(queryContext, queryCompiler));
            RegisterHandler(ToListNodeHandler.SupprotedMethods, new ToListNodeHandler(queryContext, queryCompiler));
        }

        public override Expression HandleNode(MethodCallExpression methodCall)
        {
            var handler = GetHandler(methodCall);
            if (handler != null)
            {
                return handler.HandleNode(methodCall);
            }

            var handledNode = GetNameNodeHanlder()?.HandleNode(methodCall);
            if (handledNode != null)
            {
                return handledNode;
            }

            return HandleCommonMethod(methodCall);
        }

        /// <summary>
        /// 获取根据方法名处理的Handler
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryNodeHandler GetNameNodeHanlder()
        {
            return new DefaultMethodCallNameNodeHandler(QueryContext, QueryCompiler);
        }

        protected IQueryNodeHandler GetHandler(MethodCallExpression methodCall)
        {
            //泛型方法,使用其定义
            var methodInfo = methodCall.Method;
            if (methodInfo.IsGenericMethod)
            {
                methodInfo = methodInfo.GetGenericMethodDefinition();
            }

            if (_handlers.TryGetValue(methodInfo, out IQueryNodeHandler translator))
            {
                return translator;
            }

            //if (typeof(IQuery).IsAssignableFrom(methodCall.Method.DeclaringType))
            //{
            //    return _methodCallTranslators.FirstOrDefault(i => i.Key.Name == methodCall.Method.Name).Value;
            //}

            return null;
        }

        protected virtual void RegisterHandler(IEnumerable<MethodInfo> methodInfos, IQueryNodeHandler translator)
        {
            foreach (var methodInfo in methodInfos)
            {
                _handlers[methodInfo] = translator;
            }
        }

        /// <summary>
        /// 转换普通方法调用 非Linq API
        /// </summary>
        protected virtual Expression HandleCommonMethod(MethodCallExpression methodCall)
        {
            var methodObj = Compile(methodCall.Object);
            var methodArguments = new List<Expression>();

            foreach (var item in methodCall.Arguments)
            {
                methodArguments.Add(Compile(item));
            }

            if (methodObj == null || methodObj.NodeType == ExpressionType.Constant)
            {
                var canEvacuation = true;
                var parameters = new object[methodCall.Arguments.Count];
                var obj = methodObj.As<ConstantExpression>()?.Value;

                for (var i = 0; i < methodArguments.Count; i++)
                {
                    if (methodArguments[i].NodeType != ExpressionType.Constant)
                    {
                        canEvacuation = false;
                        break;
                    }

                    parameters[i] = methodArguments[i].As<ConstantExpression>().Value;
                }

                if (canEvacuation)
                {
                    return Compile(Expression.Constant(methodCall.Method.Invoke(obj, parameters)));
                }
            }

            return Expression.Call(methodObj, methodCall.Method, methodArguments);
        }
    }
}
