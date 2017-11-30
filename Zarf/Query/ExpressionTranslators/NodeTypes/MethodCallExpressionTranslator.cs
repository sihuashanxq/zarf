using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            Register(AggregateTranslator.SupprotedMethods, new AggregateTranslator(queryContext,queryCompiper));
            Register(SelectTranslator.SupprotedMethods, new SelectTranslator(queryContext, queryCompiper));
            Register(DefaultIfEmptyTranslator.SupprotedMethods, new DefaultIfEmptyTranslator(queryContext, queryCompiper));
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
            Register(IncludeTranslator.SupprotedMethods, new IncludeTranslator(queryContext, queryCompiper));
            Register(ThenIncludeTranslator.SupprotedMethods, new ThenIncludeTranslator(queryContext, queryCompiper));
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var translator = GetTranslator(methodCall);
            if (translator != null)
            {
                return translator.Translate( methodCall);
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

            return null;
        }

        private void Register(IEnumerable<MethodInfo> methodInfos, ITranslaor translator)
        {
            foreach (var methodInfo in methodInfos)
            {
                _methodCallTranslators[methodInfo] = translator;
            }
        }

        private Expression TranslateMethodCall( MethodCallExpression methodCall)
        {
            var arguments = new List<Expression>();
            var @object = Compiler.Compile(methodCall.Object);
            var methodInfo = methodCall.Method;

            foreach (var item in methodCall.Arguments)
            {
                arguments.Add(Compiler.Compile(item));
            }

            //自定义sql函数
            var sqlFunction = methodCall.Method.GetCustomAttribute<SqlFunctionAttribute>();
            if (sqlFunction != null)
            {
                return new SqlFunctionExpression(methodCall.Method, sqlFunction.Name, @object, arguments);
            }

            if (@object == null || @object.NodeType == ExpressionType.Constant)
            {
                var parameters = new object[methodCall.Arguments.Count];
                var instance = @object.As<ConstantExpression>()?.Value;
                var canEval = true;
                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i].NodeType != ExpressionType.Constant)
                    {
                        canEval = false;
                        break;
                    }

                    parameters[i] = arguments[i].Cast<ConstantExpression>().Value;
                }

                if (canEval)
                {
                    return Compiler.Compile(Expression.Constant(methodInfo.Invoke(instance, parameters)));
                }
            }

            return Expression.Call(@object, methodInfo, arguments);
        }
    }
}
