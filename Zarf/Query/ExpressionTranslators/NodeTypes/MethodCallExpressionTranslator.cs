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

            return TranslateMethodCall(methodCall);
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

        protected virtual Expression TranslateMethodCall(MethodCallExpression methodCall)
        {
            var obj = GetCompiledExpression(methodCall.Object);

            var args = new List<Expression>();
            var methodInfo = methodCall.Method;
            foreach (var item in methodCall.Arguments)
            {
                args.Add(GetCompiledExpression(item));
            }

            //自定义sql函数
            var sqlFunction = methodCall.Method.GetCustomAttribute<SqlFunctionAttribute>();
            if (sqlFunction != null)
            {
                return new SqlFunctionExpression(methodCall.Method, sqlFunction.Name, obj, args);
            }

            if (obj == null || obj.NodeType == ExpressionType.Constant)
            {
                var parameters = new object[methodCall.Arguments.Count];
                var instance = obj.As<ConstantExpression>()?.Value;
                var canEval = true;
                for (var i = 0; i < args.Count; i++)
                {
                    if (args[i].NodeType != ExpressionType.Constant)
                    {
                        canEval = false;
                        break;
                    }

                    parameters[i] = args[i].Cast<ConstantExpression>().Value;
                }

                if (canEval)
                {
                    return Compiler.Compile(Expression.Constant(methodInfo.Invoke(instance, parameters)));
                }
            }

            return Expression.Call(obj, methodInfo, args);
        }


        public IEnumerable<Expression> UnWrap(IEnumerable<Expression> exps)
        {
            return exps.Select(item => item.UnWrap());
        }
    }
}
