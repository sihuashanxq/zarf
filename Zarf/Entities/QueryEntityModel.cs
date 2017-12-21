using System;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Entities
{
    public class QueryEntityModel
    {
        public Expression Model { get; }

        public Type ModelElementType { get; }

        public QueryEntityModel Previous { get; }

        public QueryEntityModel(Expression model, Type elementType, QueryEntityModel previous = null)
        {
            Model = model.UnWrap().As<LambdaExpression>().Body;
            ModelElementType = elementType;
            Previous = previous;
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
