using System;
using System.Linq.Expressions;

namespace Zarf.Entities
{
    public class EntityResult
    {
        public Expression EntityNewExpression { get; }

        public Type ElementType { get; }

        public EntityResult(Expression entityNewExpression, Type elemtnType)
        {
            this.EntityNewExpression = entityNewExpression;
            ElementType = elemtnType;
        }
    }
}
