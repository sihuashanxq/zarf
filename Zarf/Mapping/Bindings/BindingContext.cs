using System.Linq.Expressions;
using System;
using Zarf.Query.Expressions;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Expression Query { get; }

        public IEntityProjectionMappingProvider MappingProvider { get; set; }

        public EntityCreationHandleProvider CreationHandleProvider { get; set; }

        public BindingContext(Expression query)
        {
            Query = query;
        }
    }
}
