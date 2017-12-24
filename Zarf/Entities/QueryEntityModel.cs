using System;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Entities
{
    public class QueryEntityModel
    {
        public Expression Model { get; set; }

        public Type ModelElementType { get; set; }

        public QueryEntityModel Previous { get; set; }

        public QueryEntityModel(Expression model, Type elementType, QueryEntityModel previous = null)
        {
            Model = model.UnWrap();
            ModelElementType = elementType;
            Previous = previous;

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

        public Expression GetModelExpression(Type modelEleType)
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
    }
}
