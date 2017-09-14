using System;
using System.Linq.Expressions;
using Zarf.Mapping.Activators;
using Zarf.Extensions;
using System.Collections.Generic;

namespace Zarf.Mapping.Bindings
{
    public class EntityBinder : IEntityBinder
    {
        public void Bind(Type type, Expression bindExpression)
        {
            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(type);
            var bindings = new List<MemberBinding>();


            throw new NotImplementedException();
        }
    }
}
