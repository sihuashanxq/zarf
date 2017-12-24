using System;
using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;
using Zarf.Extensions;
using Zarf.Entities;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    class ConstantExpressionTranslator : Translator<ConstantExpression>
    {
        public ConstantExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(ConstantExpression constant)
        {
            if (!typeof(IInternalQuery).IsAssignableFrom(constant.Type))
            {
                return constant;
            }

            var typeOfEntity = constant.Type.GenericTypeArguments?[0];
            if (typeOfEntity == null)
            {
                throw new NotImplementedException("using IDataQuery<T>");
            }

            return CreateQueryExpression(typeOfEntity);
        }

        protected virtual QueryExpression CreateQueryExpression(Type typeOfEntity)
        {
            var parameter = Expression.Parameter(typeOfEntity);
            var query = new QueryExpression(typeOfEntity, Context.ColumnCaching, Context.Alias.GetNewTable());
            var modelExpression = new ModelRefrenceExpressionVisitor(Context, query, parameter).Visit(parameter);

            query.QueryModel = new QueryEntityModel(modelExpression, typeOfEntity);

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            CreateProjectionVisitor(query).Visit(modelExpression);

            return query;
        }

        protected ProjectionExpressionVisitor CreateProjectionVisitor(QueryExpression query)
        {
            return new ProjectionExpressionVisitor(query, Context);
        }
    }
}
