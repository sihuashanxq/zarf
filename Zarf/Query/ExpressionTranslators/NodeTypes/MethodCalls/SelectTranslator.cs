using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    /// <summary>
    /// Select Query
    /// </summary>
    public class SelectTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SelectTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Select")
                .Concat(new[] { ReflectionUtil.JoinSelect });
        }

        public SelectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var modelExpression = methodCall.Arguments[1];
            var modelElementType = methodCall.Method.ReturnType.GetModelElementType();
            var parameter = modelExpression.GetParameters().FirstOrDefault();

            Utils.CheckNull(query, "query");

            query.QueryModel = new QueryEntityModel(modelExpression, modelElementType, query.QueryModel);

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            CreateProjectionVisitor(query).Visit(modelExpression);

            return query;
        }

        protected ProjectionExpressionVisitor CreateProjectionVisitor(QueryExpression query)
        {
            return new ProjectionExpressionVisitor(query, Context);
        }

        protected virtual void RegisterJoinSelectQueries(QueryExpression query, Expression selector)
        {
            var parameters = GetParameteres(selector);
            var i = 0;
            while (i < parameters.Count)
            {
                if (i == 0)
                    MapParameterWithQuery(parameters[i++], query);
                else
                    MapParameterWithQuery(parameters[i], query.Joins[i++ - 1].Query);
            }
        }
    }
}