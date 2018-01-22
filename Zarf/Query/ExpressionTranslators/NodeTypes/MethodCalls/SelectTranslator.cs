using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    /// <summary>
    /// Select Query
    /// </summary>
    public class SelectTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SelectTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Select");
        }

        public SelectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression selector, MethodInfo method)
        {
            var modelType = method.ReturnType.GetModelElementType();
            var parameter = selector.GetParameters().FirstOrDefault();
            var modelExpression = new ModelRefrenceExpressionVisitor(QueryContext, select, parameter).Visit(selector);

            Utils.CheckNull(select, "query");

            select.QueryModel = new QueryEntityModel(select, modelExpression, modelType, select.QueryModel);

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            CreateProjection(select, modelExpression);

            if (select.QueryModel.Model.Is<ConstantExpression>())
            {
                select.AddProjection(new AliasExpression(QueryContext.AliasGenerator.GetNewColumn(), select.QueryModel.Model, selector));
            }

            return select;
        }

        protected void CreateProjection(SelectExpression select, Expression modelExpression)
        {
            modelExpression = new ProjectionExpressionVisitor(select, QueryContext).Visit(modelExpression);

            new ResultExpressionVisitor(QueryContext, select).Visit(modelExpression);
        }
    }
}