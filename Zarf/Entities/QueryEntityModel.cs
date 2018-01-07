using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Entities
{
    public class QueryEntityModelRefrenceOuterColumn
    {
        public MemberInfo Member { get; set; }

        public ColumnExpression RefrencedColumn { get; set; }
    }

    public class QueryEntityModel
    {
        public QueryExpression Query { get; set; }

        public Type ModelType { get; set; }

        public Expression Model { get; set; }

        public Type ModelElementType => ModelType.GetModelElementType();

        public QueryEntityModel Previous { get; set; }

        public List<QueryEntityModelRefrenceOuterColumn> RefrencedColumns { get; }

        public QueryEntityModel(QueryExpression query, Expression model, Type modelType, QueryEntityModel previous = null)
        {
            Query = query;
            Model = model.UnWrap();
            ModelType = modelType;
            Previous = previous;
            RefrencedColumns = new List<QueryEntityModelRefrenceOuterColumn>();
            if (Model.NodeType == ExpressionType.Lambda)
            {
                Model = Model.As<LambdaExpression>().Body;
            }
        }

        public bool ContainsModel(Expression modelExpresion)
        {
            if (Model == modelExpresion)
            {
                return true;
            }

            if (Previous != null)
            {
                return Previous.ContainsModel(modelExpresion);
            }

            return false;
        }

        public Expression GetModelExpression(Type modelEleType, MemberInfo memberInfo = null)
        {
            if (ModelElementType == modelEleType)
            {
                return Model;
            }

            if (Previous != null)
            {
                return Previous.GetModelExpression(modelEleType);
            }

            return null;
        }

        public QueryEntityModel FindQueryModel(Expression modelExpression)
        {
            if (Model == modelExpression)
            {
                return this;
            }

            return Previous?.FindQueryModel(modelExpression);
        }
    }
}
