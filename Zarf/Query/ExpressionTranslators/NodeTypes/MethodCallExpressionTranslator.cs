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
        private static readonly Dictionary<MethodInfo, ITranslaor> _methodCallTranslators = new Dictionary<MethodInfo, ITranslaor>();

        static MethodCallExpressionTranslator()
        {
            Register(AggregateTranslator.SupprotedMethods, new AggregateTranslator());
            Register(SelectTranslator.SupprotedMethods, new SelectTranslator());
            Register(DefaultIfEmptyTranslator.SupprotedMethods, new DefaultIfEmptyTranslator());
            Register(DistinctTranslator.SupprotedMethods, new DistinctTranslator());
            Register(ExceptTranslator.SupprotedMethods, new ExceptTranslator());
            Register(FirstTranslator.SupprotedMethods, new FirstTranslator());
            Register(GroupByTranslator.SupprotedMethods, new GroupByTranslator());
            Register(JoinTranslator.SupprotedMethods, new JoinTranslator());
            Register(OrderByTranslator.SupprotedMethods, new OrderByTranslator());
            Register(SingleTranslator.SupprotedMethods, new SingleTranslator());
            Register(SkipTranslator.SupprotedMethods, new SkipTranslator());
            Register(TakeTranslator.SupprotedMethods, new TakeTranslator());
            Register(UnionTranslator.SupprotedMethods, new UnionTranslator());
            Register(WhereTranslator.SupprotedMethods, new WhereTranslator());
            Register(AllTranslator.SupprotedMethods, new AllTranslator());
            Register(AnyTranslator.SupprotedMethods, new AnyTranslator());
            Register(IncludeTranslator.SupprotedMethods, new IncludeTranslator());
            Register(ThenIncludeTranslator.SupprotedMethods, new ThenIncludeTranslator());
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var translator = GetTranslator(methodCall);
            if (translator != null)
            {
                return translator.Translate(context, methodCall, transformVisitor);
            }

            return TranslateMethodCall(context, methodCall, transformVisitor);
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

        private static void Register(IEnumerable<MethodInfo> methodInfos, ITranslaor translator)
        {
            foreach (var methodInfo in methodInfos)
            {
                _methodCallTranslators[methodInfo] = translator;
            }
        }

        private Expression TranslateMethodCall(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var arguments = new List<Expression>();
            var @object = transformVisitor.Visit(methodCall.Object);
            var methodInfo = methodCall.Method;

            foreach (var item in methodCall.Arguments)
            {
                arguments.Add(transformVisitor.Visit(item));
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

                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i].NodeType != ExpressionType.Constant)
                    {
                        break;
                    }

                    parameters[i] = arguments[i].Cast<ConstantExpression>().Value;
                }

                return transformVisitor.Visit(Expression.Constant(methodInfo.Invoke(instance, parameters)));
            }

            return Expression.Call(@object, methodInfo, arguments);
        }
    }
}
