using System;
using System.Linq.Expressions;

namespace Zarf.Entities
{
    public class QueryEntityModel
    {
        public Expression Model { get; }

        public Type ModelElementType { get; }

        public QueryEntityModel(Expression model, Type elementType)
        {
            Model = model;
            ModelElementType = elementType;
        }
    }
}
