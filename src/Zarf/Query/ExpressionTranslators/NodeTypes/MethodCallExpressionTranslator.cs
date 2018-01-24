using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.ExpressionTranslators.Methods;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MethodCallExpressionTranslator : Translator<MethodCallExpression>
    {
        /// <summary>
        /// 只查询转换
        /// </summary>
        protected ITranslator SubQueryTranslator { get; set; }

        private readonly Dictionary<MethodInfo, ITranslator> _methodCallTranslators = new Dictionary<MethodInfo, ITranslator>();

        public MethodCallExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            RegisterTranslator(AggregateTranslator.SupprotedMethods, new AggregateTranslator(queryContext, queryCompiper));
            RegisterTranslator(SelectTranslator.SupprotedMethods, new SelectTranslator(queryContext, queryCompiper));
            RegisterTranslator(DistinctTranslator.SupprotedMethods, new DistinctTranslator(queryContext, queryCompiper));
            RegisterTranslator(ExceptTranslator.SupprotedMethods, new ExceptTranslator(queryContext, queryCompiper));
            RegisterTranslator(FirstTranslator.SupprotedMethods, new FirstTranslator(queryContext, queryCompiper));
            RegisterTranslator(GroupByTranslator.SupprotedMethods, new GroupByTranslator(queryContext, queryCompiper));
            RegisterTranslator(JoinTranslator.SupprotedMethods, new JoinTranslator(queryContext, queryCompiper));
            RegisterTranslator(OrderByTranslator.SupprotedMethods, new OrderByTranslator(queryContext, queryCompiper));
            RegisterTranslator(SingleTranslator.SupprotedMethods, new SingleTranslator(queryContext, queryCompiper));
            RegisterTranslator(SkipTranslator.SupprotedMethods, new SkipTranslator(queryContext, queryCompiper));
            RegisterTranslator(TakeTranslator.SupprotedMethods, new TakeTranslator(queryContext, queryCompiper));
            RegisterTranslator(UnionTranslator.SupprotedMethods, new UnionTranslator(queryContext, queryCompiper));
            RegisterTranslator(WhereTranslator.SupprotedMethods, new WhereTranslator(queryContext, queryCompiper));
            RegisterTranslator(AllTranslator.SupprotedMethods, new AllTranslator(queryContext, queryCompiper));
            RegisterTranslator(AnyTranslator.SupprotedMethods, new AnyTranslator(queryContext, queryCompiper));
            RegisterTranslator(JoinSelectTranslator.SupprotedMethods, new JoinSelectTranslator(queryContext, queryCompiper));
            RegisterTranslator(ToListTranslator.SupprotedMethods, new ToListTranslator(queryContext, queryCompiper));
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var translator = GetTranslator(methodCall);
            if (translator != null)
            {
                return translator.Translate(methodCall);
            }

            var trySubQuery = SubQueryTranslator?.Translate(methodCall);
            if (trySubQuery != null)
            {
                return trySubQuery;
            }

            return TranslateCommonMethod(methodCall);
        }

        protected ITranslator GetTranslator(MethodCallExpression methodCall)
        {
            //泛型方法,使用其定义
            var methodInfo = methodCall.Method;
            if (methodInfo.IsGenericMethod)
            {
                methodInfo = methodInfo.GetGenericMethodDefinition();
            }

            if (_methodCallTranslators.TryGetValue(methodInfo, out ITranslator translator))
            {
                return translator;
            }

            //if (typeof(IQuery).IsAssignableFrom(methodCall.Method.DeclaringType))
            //{
            //    return _methodCallTranslators.FirstOrDefault(i => i.Key.Name == methodCall.Method.Name).Value;
            //}

            return null;
        }

        protected virtual void RegisterTranslator(IEnumerable<MethodInfo> methodInfos, ITranslator translator)
        {
            foreach (var methodInfo in methodInfos)
            {
                _methodCallTranslators[methodInfo] = translator;
            }
        }

        /// <summary>
        /// 转换普通方法调用 非Linq API
        /// </summary>
        protected virtual Expression TranslateCommonMethod(MethodCallExpression methodCall)
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
