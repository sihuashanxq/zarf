using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.Methods;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MethodCallExpressionTranslator : Translator<MethodCallExpression>
    {
        private readonly Dictionary<MethodInfo, ITranslaor> _methodCallTranslators = new Dictionary<MethodInfo, ITranslaor>();

        public MethodCallExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            Register(AggregateTranslator.SupprotedMethods, new AggregateTranslator(queryContext, queryCompiper));
            Register(SelectTranslator.SupprotedMethods, new SelectTranslator(queryContext, queryCompiper));
            Register(DistinctTranslator.SupprotedMethods, new DistinctTranslator(queryContext, queryCompiper));
            Register(ExceptTranslator.SupprotedMethods, new ExceptTranslator(queryContext, queryCompiper));
            Register(FirstTranslator.SupprotedMethods, new FirstTranslator(queryContext, queryCompiper));
            Register(GroupByTranslator.SupprotedMethods, new GroupByTranslator(queryContext, queryCompiper));
            Register(JoinTranslator.SupprotedMethods, new JoinTranslator(queryContext, queryCompiper));
            Register(OrderByTranslator.SupprotedMethods, new OrderByTranslator(queryContext, queryCompiper));
            Register(SingleTranslator.SupprotedMethods, new SingleTranslator(queryContext, queryCompiper));
            Register(SkipTranslator.SupprotedMethods, new SkipTranslator(queryContext, queryCompiper));
            Register(TakeTranslator.SupprotedMethods, new TakeTranslator(queryContext, queryCompiper));
            Register(UnionTranslator.SupprotedMethods, new UnionTranslator(queryContext, queryCompiper));
            Register(WhereTranslator.SupprotedMethods, new WhereTranslator(queryContext, queryCompiper));
            Register(AllTranslator.SupprotedMethods, new AllTranslator(queryContext, queryCompiper));
            Register(AnyTranslator.SupprotedMethods, new AnyTranslator(queryContext, queryCompiper));
            Register(JoinSelectTranslator.SupprotedMethods, new JoinSelectTranslator(queryContext, queryCompiper));
            Register(ToListTranslator.SupprotedMethods, new ToListTranslator(queryContext, queryCompiper));
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var translator = GetTranslator(methodCall);
            if (translator != null)
            {
                return translator.Translate(methodCall);
            }

            var trySubQuery = new SubQueryTranslator(Context, Compiler).Translate(methodCall);
            if (trySubQuery != null)
            {
                return trySubQuery;
            }

            return TryInvokeConstantMethodCall(methodCall);
        }

        private ITranslaor GetTranslator(MethodCallExpression methodCall)
        {
            //泛型方法,使用其定义
            var methodInfo = methodCall.Method;
            if (methodInfo.IsGenericMethod)
            {
                methodInfo = methodInfo.GetGenericMethodDefinition();
            }

            if (_methodCallTranslators.TryGetValue(methodInfo, out ITranslaor translator))
            {
                return translator;
            }

            try
            {
                if (methodCall.Method.DeclaringType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IQuery<>))
                    && methodCall.Method.Name == "ToList")
                {
                    return _methodCallTranslators[ToListTranslator.SupprotedMethods.FirstOrDefault()];
                }
            }

            catch { }
            return null;
        }

        private void Register(IEnumerable<MethodInfo> methodInfos, ITranslaor translator)
        {
            foreach (var methodInfo in methodInfos)
            {
                _methodCallTranslators[methodInfo] = translator;
            }
        }

        /// <summary>
        /// 尝试调用方法
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual Expression TryInvokeConstantMethodCall(MethodCallExpression methodCall)
        {
            var methodObj = GetCompiledExpression(methodCall.Object);
            var methodArguments = new List<Expression>();

            foreach (var item in methodCall.Arguments)
            {
                methodArguments.Add(GetCompiledExpression(item));
            }

            if (methodObj == null || methodObj.NodeType == ExpressionType.Constant)
            {
                var canInvoke = true;
                var parameters = new object[methodCall.Arguments.Count];
                var obj = methodObj.As<ConstantExpression>()?.Value;

                for (var i = 0; i < methodArguments.Count; i++)
                {
                    if (methodArguments[i].NodeType != ExpressionType.Constant)
                    {
                        canInvoke = false;
                        break;
                    }

                    parameters[i] = methodArguments[i].As<ConstantExpression>().Value;
                }

                if (canInvoke)
                {
                    return GetCompiledExpression(Expression.Constant(methodCall.Method.Invoke(obj, parameters)));
                }
            }

            return Expression.Call(methodObj, methodCall.Method, methodArguments);
        }
    }
}
