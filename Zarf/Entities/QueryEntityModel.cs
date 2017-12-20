using System;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Entities
{
    public class QueryEntityModel
    {
        public Expression Model { get; }

        public Type ModelElementType { get; }

        public QueryEntityModel(Expression model, Type elementType)
        {
            Model = model.UnWrap().As<LambdaExpression>().Body;
            ModelElementType = elementType;
        }
    }
}
