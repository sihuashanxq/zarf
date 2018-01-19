using System;
using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Queries.Expressions;
using Zarf.Queries.ExpressionVisitors;
using Zarf.Entities;

namespace Zarf.Queries.ExpressionTranslators.NodeTypes
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

            return CreateQueryExpression(typeOfEntity, constant);
        }

        protected virtual QueryExpression CreateQueryExpression(Type typeOfEntity, Expression constant)
        {
            var parameter = Expression.Parameter(typeOfEntity, Context.Alias.GetNewParameter());
            var query = new QueryExpression(typeOfEntity, Context.ExpressionMapper, Context.Alias.GetNewTable());
            var modelExpression = new ModelRefrenceExpressionVisitor(Context, query, parameter).Visit(parameter);

            query.QueryModel = new QueryEntityModel(query, modelExpression, constant.Type);

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);
            Context.QueryModelMapper.MapQueryModel(constant, query.QueryModel);

            CreateProjectionVisitor(query).Visit(modelExpression);

            return query;
        }

        protected ProjectionExpressionVisitor CreateProjectionVisitor(QueryExpression query)
        {
            return new ProjectionExpressionVisitor(query, Context);
        }
    }
}
