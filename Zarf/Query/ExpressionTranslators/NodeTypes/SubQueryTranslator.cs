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
            var obj = Compile(methodCall.Object);
            if (obj is ConstantExpression constant && constant.Value is IQuery)
            {
                obj = InvokeSubQueryMethodCall(constant, methodCall);
            }

            if (obj.As<ConstantExpression>()?.Value is JoinQuery joinQuery)
            {
                return new JoinTranslator(QueryContext, QueryCompiler).Transalte(joinQuery);
            }

            if (obj is MethodCallExpression call && call.Method.DeclaringType == typeof(QueryableDefinition))
            {
                return Compile(obj);
            }

            var select = obj as SelectExpression;
            if (select == null)
            {
                return select;
            }

            var methodName = methodCall.Method.Name;

            if (methodName == "All")
            {
                select = new AllTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);

                var allExpresion = new AllExpression(select);

                QueryContext.ExpressionMapper.Map(allExpresion, Utils.ExpressionConstantTrue);

                return allExpresion;
            }

            if (methodName == "Any")
            {
                select = new AnyTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);

                var anyExpression = new AnyExpression(select);

                QueryContext.ExpressionMapper.Map(anyExpression, Utils.ExpressionConstantTrue);

                return anyExpression;
            }

            if (methodName == "Select")
            {
                if (typeof(IJoinQuery).IsAssignableFrom(methodCall.Method.DeclaringType))
                {
                    select = new JoinSelectTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
                }
                else
                {
                    select = new SelectTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
                }
            }

            if (methodName == "Where")
            {
                select = new WhereTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
            }

            if (new[] { "Count", "LongCount", "Sum", "Max", "Min", "Average" }.Contains(methodName))
            {
                select = new AggregateTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments.Count == 0 ? null : methodCall.Arguments[0], methodCall.Method);
            }

            if (new[] { "First", "FirstOrDefault", "Single", "SingleOrDefault" }.Contains(methodCall.Method.Name))
            {
                select = new FirstTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments.Count == 1 ? methodCall.Arguments[0] : null, methodCall.Method);
            }

            if (methodName == "Skip")
            {
                select = new SkipTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
            }

            if (methodName == "Take")
            {
                select = new TakeTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[0], methodCall.Method);
            }

            if (methodName == "OrderBy")
            {
                select = new OrderByTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodName == "GroupBy")
            {
                select = new GroupByTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodName == "Union" || methodName == "Concat")
            {
                select = new UnionTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodName == "Except")
            {
                select = new ExceptTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            if (methodName == "Intersect")
            {
                select = new IntersectTranslator(QueryContext, QueryCompiler).Translate(select, methodCall.Arguments[1], methodCall.Method);
            }

            select.QueryModel.ModelType = methodCall.Method.ReturnType;

            QueryContext.QueryModelMapper.MapQueryModel(methodCall, select.QueryModel);

            return select;
        }

        protected Expression InvokeSubQueryMethodCall(Expression obj, MethodCallExpression methodCall)
        {
            //join 特殊处理,不支持外部参数引用
            var method = methodCall.Method;

            if (typeof(IQuery).IsAssignableFrom(method.DeclaringType) && method.Name == "Join")
            {
                var joinBody = Expression.Call(obj, method, methodCall.Arguments);
                var joinQuery = Expression.Lambda(joinBody).Compile().DynamicInvoke();

                return Compile(Expression.Constant(joinQuery));
            }

            var iQuery = obj.As<ConstantExpression>()?.Value as IQuery;
            if (iQuery == null)
            {
                throw new NullReferenceException("subquery refrence null!");
            }

            var typeOfEntity = iQuery.As<Core.Query>().InternalQuery.ElementType;
            var callArgumnets = new[] { iQuery.As<Core.Query>().InternalQuery.Expression };
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
            var conds = QueryableDefinition.Methods.Where(item => item.Name == method.Name).ToList();

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
