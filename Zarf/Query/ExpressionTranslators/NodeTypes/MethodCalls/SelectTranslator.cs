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
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Select");
        }

        public SelectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var modelElementType = methodCall.Method.ReturnType.GetModelElementType();
            var parameter = methodCall.Arguments[1].GetParameters().FirstOrDefault();
            var modelExpression = new ModelRefrenceExpressionVisitor(Context, query, parameter).Visit(methodCall.Arguments[1]);

            Utils.CheckNull(query, "query");

            query.QueryModel = new QueryEntityModel(modelExpression, modelElementType, query.QueryModel);

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            CreateProjection(query, modelExpression);

            if (query.QueryModel.Model.Is<ConstantExpression>())
            {
                query.AddProjection(new AliasExpression(Context.Alias.GetNewColumn(), query.QueryModel.Model, methodCall.Arguments[1]));
            }

            var s = Context.DbContextParts.CommandTextBuilder.Build(query);

            return query;
        }

        protected void CreateProjection(QueryExpression query, Expression modelExpression)
        {
            modelExpression = new ProjectionExpressionVisitor(query, Context).Visit(modelExpression);

            new ResultExpressionVisitor(query).Visit(modelExpression);
        }
    }
}