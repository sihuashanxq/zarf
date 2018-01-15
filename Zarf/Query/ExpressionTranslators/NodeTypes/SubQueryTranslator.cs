using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

            if (!(obj is QueryExpression)) return null;

            var query = obj as QueryExpression;

            if (methodCall.Method.Name == "Select" && typeof(IJoinQuery).IsAssignableFrom(methodCall.Method.DeclaringType))
            {
                query = new JoinSelectTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
            }

            if (methodCall.Method.Name == "Where")
            {
                query = new WhereTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
            }

            if (new[] { "Count", "LongCount", "Sum", "Max", "Min", "Average" }.Contains(methodCall.Method.Name))
            {
                query = new AggregateTranslator(Context, Compiler).Translate(query, methodCall.Arguments.Count == 0 ? null : methodCall.Arguments[0], methodCall.Method);
            }

            if (methodCall.Method.Name == "Select")
            {
                query = new SelectTranslator(Context, Compiler).Translate(query, methodCall.Arguments[0]);
            }

            if (new[] { "First", "FirstOrDefault", "Single", "SingleOrDefault" }.Contains(methodCall.Method.Name))
            {
                //
            }

            if (methodCall.Method.Name == "All")
            {
                //
            }

            if (methodCall.Method.Name == "Any")
            {
                //
            }

            if (methodCall.Method.Name == "Skip")
            {

            }

            if (methodCall.Method.Name == "Take")
            {

            }

            if (methodCall.Method.Name == "OrderBy")
            {

            }

            if (methodCall.Method.Name == "GroupBy")
            {

            }

            Context.QueryModelMapper.MapQueryModel(methodCall, query.QueryModel);

            return query;
        }

        protected Expression InvokeSubQueryMethodCall(Expression obj, MethodCallExpression methodCall)
        {
            //join 特殊处理,不支持外部参数引用
            if (typeof(IQuery).IsAssignableFrom(methodCall.Method.DeclaringType) &&
                methodCall.Method.Name == "Join")
            {
                var body = Expression.Call(obj, methodCall.Method, methodCall.Arguments);
                var queryObj = Expression.Lambda(body).Compile().DynamicInvoke();

                return GetCompiledExpression(Expression.Constant(queryObj));
            }

            var iQuery = obj.As<ConstantExpression>()?.Value as IQuery;
            var typeOfEntity = iQuery?.GetInternalQuery().GetTypeOfEntity();
            var queryExpression = new[] { iQuery?.GetInternalQuery()?.GetExpression() };

            var method = ReflectionUtil.FindQueryableMethod(methodCall.Method, typeOfEntity, methodCall.Method.ReturnType.GetModelElementType());
            return Expression.Call(null, method, queryExpression.Concat(methodCall.Arguments).ToArray());
        }
    }
}
