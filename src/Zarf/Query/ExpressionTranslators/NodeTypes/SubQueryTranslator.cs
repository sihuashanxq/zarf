using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    /// <summary>
    /// ugly
    /// </summary>
    public class SubQueryTranslator : Translator<MethodCallExpression>
    {
        public SubQueryTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var obj = Compile(methodCall.Object);
            if (obj is ConstantExpression constant && constant.Value is IQuery)
            {
                obj = TranslateQueryMethodCall(constant, methodCall);
            }

            if (obj.As<ConstantExpression>()?.Value is JoinQuery joinQuery)
            {
                return new JoinTranslator(QueryContext, QueryCompiler).Transalte(joinQuery);
            }

            if (obj is MethodCallExpression call && call.Method.DeclaringType == typeof(QueryableDefinition))
            {
                return Compile(obj);
            }

            if (!(obj is SelectExpression select))
            {
                return obj;
            }

            select = Translate(select, methodCall.Arguments.Count == 1 ? methodCall.Arguments[0] : null, methodCall.Method);

            if (methodCall.Method.Name == "All" || methodCall.Method.Name == "Any")
            {
                var exp = methodCall.Method.Name == "All"
                    ? (Expression)new AllExpression(select)
                    : (Expression)new AnyExpression(select);

                QueryContext.ExpressionMapper.Map(exp, Utils.ExpressionConstantTrue);

                return exp;
            }

            QueryContext.ModelMapper.Map(methodCall, select.QueryModel);

            return select;
        }

        /// <summary>
        /// </summary>
        /// <param methodCall.Method.Name="obj"><see cref="IQuery"/></param>
        /// <param methodCall.Method.Name="methodCall"></param>
        /// <returns></returns>
        protected Expression TranslateQueryMethodCall(Expression obj, MethodCallExpression methodCall)
        {
            //join 特殊处理,不支持外部参数引用
            if (typeof(IQuery).IsAssignableFrom(methodCall.Method.DeclaringType) && methodCall.Method.Name == "Join")
            {
                var joinBody = Expression.Call(obj, methodCall.Method, methodCall.Arguments);
                var joinQuery = Expression.Lambda(joinBody).Compile().DynamicInvoke();

                return Compile(Expression.Constant(joinQuery));
            }

            var method = methodCall.Method;
            var iQuery = obj.As<ConstantExpression>()?.Value as IQuery;
            if (iQuery == null)
            {
                throw new NullReferenceException("subquery refrence null!");
            }

            var elementType = iQuery.As<Zarf.Core.Query>().InternalQuery.ElementType;
            var expression = iQuery.As<Zarf.Core.Query>().InternalQuery.Expression;

            var queryMethod = FindQueryableMethod(
                method,
                elementType,
                method.ReturnType.GetModelElementType());

            if (queryMethod == null)
            {
                throw new NullReferenceException("not found subquery method!");
            }

            return Expression.Call(null, queryMethod, new[] { expression }.Concat(methodCall.Arguments).ToArray());
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
