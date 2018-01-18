using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.Methods;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class SubQueryTranslator : Translator<MethodCallExpression>
    {
        protected Dictionary<string, ITranslaor> Translators { get; }

        public SubQueryTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            Translators = new Dictionary<string, ITranslaor>();
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var obj = GetCompiledExpression(methodCall.Object);
            if (obj is ConstantExpression constant && constant.Value is IQuery)
            {
                obj = InvokeSubQueryMethodCall(constant, methodCall);
            }

            if (obj.As<ConstantExpression>()?.Value is JoinQuery joinQuery)
            {
                return new JoinTranslator(Context, Compiler).Transalte(joinQuery);
            }

            if (obj is MethodCallExpression call && call.Method.DeclaringType == typeof(ZarfQueryable))
            {
                return GetCompiledExpression(obj);
            }

            var query = obj as QueryExpression;
            if (query == null)
            {
                return query;
            }

            var methodName = methodCall.Method.Name;

            if (methodName == "All")
            {
                query = new AllTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);

                var allExpresion = new AllExpression(query);

                Context.ExpressionMapper.Map(allExpresion, Utils.ExpressionConstantTrue);

                return allExpresion;
            }

            if (methodName == "Any")
            {
                query = new AnyTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);

                var anyExpression = new AnyExpression(query);

                Context.ExpressionMapper.Map(anyExpression, Utils.ExpressionConstantTrue);

                return anyExpression;
            }

            if (methodName == "Select")
            {
                if (typeof(IJoinQuery).IsAssignableFrom(methodCall.Method.DeclaringType))
                {
                    query = new JoinSelectTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
                }
                else
                {
                    query = new SelectTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
                }
            }

            if (methodName == "Where")
            {
                query = new WhereTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
            }

            if (new[] { "Count", "LongCount", "Sum", "Max", "Min", "Average" }.Contains(methodName))
            {
                query = new AggregateTranslator(Context, Compiler).Translate(query, methodCall.Arguments.Count == 0 ? null : methodCall.Arguments[0], methodCall.Method);
            }

            if (new[] { "First", "FirstOrDefault", "Single", "SingleOrDefault" }.Contains(methodCall.Method.Name))
            {
                query = new FirstTranslator(Context, Compiler).Translate(query, methodCall.Arguments.Count == 1 ? methodCall.Arguments[0] : null);
            }

            if (methodName == "Skip")
            {
                query = new SkipTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
            }

            if (methodName == "Take")
            {
                query = new TakeTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
            }

            if (methodName == "OrderBy")
            {
                query = new OrderByTranslator(Context, Compiler).Translate(query, methodCall.Arguments[1]);
            }

            if (methodName == "GroupBy")
            {
                query = new GroupByTranslator(Context, Compiler).Translate(query, methodCall.Arguments[1]);
            }

            if (methodName == "Union"|| methodName == "Concat")
            {
                query = new UnionTranslator(Context, Compiler).Translate(query, methodCall.Arguments[1]);
            }

            if (methodName == "Except")
            {
                query = new ExceptTranslator(Context, Compiler).Translate(query, methodCall.Arguments[1]);
            }

            if (methodName == "Intersect")
            {
                query = new IntersectTranslator(Context, Compiler).Translate(query, methodCall.Arguments[1]);
            }

            query.QueryModel.ModelType = methodCall.Method.ReturnType;

            Context.QueryModelMapper.MapQueryModel(methodCall, query.QueryModel);

            return query;
        }

        protected Expression InvokeSubQueryMethodCall(Expression obj, MethodCallExpression methodCall)
        {
            //join 特殊处理,不支持外部参数引用
            var method = methodCall.Method;

            if (typeof(IQuery).IsAssignableFrom(method.DeclaringType) && method.Name == "Join")
            {
                var joinBody = Expression.Call(obj, method, methodCall.Arguments);
                var joinQuery = Expression.Lambda(joinBody).Compile().DynamicInvoke();

                return GetCompiledExpression(Expression.Constant(joinQuery));
            }

            var iQuery = obj.As<ConstantExpression>()?.Value as IQuery;
            if (iQuery == null)
            {
                throw new NullReferenceException("subquery refrence null!");
            }

            var typeOfEntity = iQuery.GetInternalQuery().GetTypeOfEntity();
            var callArgumnets = new[] { iQuery.GetInternalQuery().GetExpression() };
            var typeOfResult = method.ReturnType.GetModelElementType();

            var queryMethod = FindQueryableMethod(method, typeOfEntity, typeOfResult);
            if (queryMethod == null)
            {
                throw new NullReferenceException("not found subquery method!");
            }

            return Expression.Call(null, queryMethod, callArgumnets.Concat(methodCall.Arguments).ToArray());
        }

        public static MethodInfo FindQueryableMethod(MethodInfo method, Type typeOfEntity, Type typeOfResult)
        {
            Func<MethodInfo, MethodInfo> makeGenericMethod = (m) =>
            {
                if (!m.IsGenericMethod)
                {
                    return m;
                }

                if (m.GetGenericArguments().Length == 2)
                {
                    return m.MakeGenericMethod(typeOfEntity, typeOfResult);
                }

                return m.MakeGenericMethod(typeOfEntity);
            };

            var parameters = method.GetParameters();
            var conds = ZarfQueryable.Methods.Where(item => item.Name == method.Name).ToList();

            foreach (var cond in conds)
            {
                var genericCondMethod = makeGenericMethod(cond);
                var genericCondParameters = genericCondMethod.GetParameters();
                if (genericCondParameters.Length != parameters.Length + 1)
                {
                    continue;
                }

                var i = 1;
                while (i < genericCondParameters.Length)
                {
                    if (genericCondParameters[i].ParameterType != parameters[i - 1].ParameterType)
                    {
                        break;
                    }
                    i++;
                }

                if (i >= genericCondParameters.Length)
                {
                    return genericCondMethod;
                }
            }

            throw new Exception($"can not find {method.Name}the mapped Queryable Method");
        }
    }
}
