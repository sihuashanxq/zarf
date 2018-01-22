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
    /// Join Select Query
    /// </summary>
    public class JoinSelectTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static JoinSelectTranslator()
        {
            SupprotedMethods = new[] { ReflectionUtil.JoinSelectMethod };
        }

        public JoinSelectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override SelectExpression Translate(SelectExpression select, Expression selector, MethodInfo method)
        {
            var modelType = selector.Type;
            var parameters = selector.GetParameters().ToList();
            var modelExpression = new ModelRefrenceExpressionVisitor(QueryContext, select, parameters[0]).Visit(selector);

            select.QueryModel = new QueryEntityModel(select, modelExpression, modelType, select.QueryModel);

            for (var i = 0; i < parameters.Count; i++)
            {
                if (i != 0)
                {
                    QueryContext.SelectMapper.Map(parameters[i], select.Joins[i - 1].Select);
                    QueryContext.ModelMapper.Map(parameters[i], select.Joins[i - 1].Select.QueryModel);
                    continue;
                }

                QueryContext.SelectMapper.Map(parameters[i], select);
                QueryContext.ModelMapper.Map(parameters[i], select.QueryModel);
            }

            Utils.CheckNull(select, "query");

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