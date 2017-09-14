using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Zarf.Mapping.Activators
{
    public class EntityPropertyBinder : IEntityMemberBinder<PropertyInfo>
    {
        public void Bind(PropertyInfo member, Expression bindExpression)
        {
            throw new NotImplementedException();
        }
    }
}
