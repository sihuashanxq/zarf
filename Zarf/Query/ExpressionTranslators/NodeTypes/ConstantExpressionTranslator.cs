using System;
using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class ConstantExpressionTranslator : Translator<ConstantExpression>
    {
        public ConstantExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(ConstantExpression constant)
        {
            if (!typeof(IInternalQuery).IsAssignableFrom(constant.Type) &&
                !typeof(IQuery).IsAssignableFrom(constant.Type))
            {
                return constant;
            }

            var typeOfEntity = constant.Type.GenericTypeArguments?[0];
            if (typeOfEntity == null)
            {
                throw new NotImplementedException("using IDataQuery<T>");
            }

            return CreateSelectExpression(typeOfEntity, constant);
        }

        protected virtual SelectExpression CreateSelectExpression(Type typeOfEntity, Expression constant)
        {
            var parameter = Expression.Parameter(typeOfEntity, QueryContext.AliasGenerator.GetNewParameter());
            var select = new SelectExpression(typeOfEntity, QueryContext.ExpressionMapper, QueryContext.AliasGenerator.GetNewTable());
            var modelExpression = new QueryModelExpandExpressionVisitor(QueryContext, select, parameter).Visit(parameter);

            select.QueryModel = new QueryEntityModel(select, modelExpression, constant.Type);

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);
            QueryContext.ModelMapper.Map(constant, select.QueryModel);

            CreateProjectionVisitor(select).Visit(modelExpression);

            return select;
        }

        protected ProjectionExpressionVisitor CreateProjectionVisitor(SelectExpression select)
        {
            return new ProjectionExpressionVisitor(select, QueryContext);
        }
    }
}
