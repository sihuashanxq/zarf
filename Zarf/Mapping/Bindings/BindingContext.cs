using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Type Type { get; }

        public Expression BindExpression { get; }

        public IEntityProjectionMappingProvider MappingProvider { get; set; }

        public EntityCreationHandleProvider CreationHandleProvider { get; set; }

        public BindingContext(Type type, Expression bindExpression)
        {
            Type = type;
            BindExpression = bindExpression;
        }
    }
}
