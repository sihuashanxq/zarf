using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Expression Expression { get; }

        public IEntityProjectionMappingProvider MappingProvider { get; set; }

        public EntityCreationHandleProvider CreationHandleProvider { get; set; }

        public BindingContext(Expression exp)
        {
            Expression = exp;
        }
    }
}
