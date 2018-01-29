using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.Visitors;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    /// <summary>
    /// Select Query
    /// </summary>
    public class SelectNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SelectNodeHandler()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Select");
        }

        public SelectNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression HandleNode(SelectExpression select, Expression selector, MethodInfo method)
        {
            var parameter = selector.GetParameters().FirstOrDefault();
            var modelExpression = new QueryModelExpandExpressionVisitor(QueryContext, select, parameter).Visit(selector);

            Utils.CheckNull(select, "query");

            select.QueryModel = new QueryEntityModel(select, modelExpression, method.DeclaringType, select.QueryModel);

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
            modelExpression = new ResultExpressionVisitor(QueryContext, select).Visit(modelExpression);
        }
    }
}