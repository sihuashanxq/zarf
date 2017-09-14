using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Mapping.Activators;

namespace Zarf.Mapping.Bindings
{
    public class EntityBinder : IEntityBinder
    {
        public IBindingContext BindingContext { get; }

        public EntityBinder(IBindingContext bindingContext)
        {
            BindingContext = bindingContext;
        }

        public Expression Bind(Type type, Expression entityNewExpression)
        {
            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(type);
            var bindings = new List<MemberBinding>();

            throw new NotImplementedException();
        }
    }
}
