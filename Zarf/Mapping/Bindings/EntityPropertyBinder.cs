using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Zarf.Mapping.Activators
{
    public class EntityPropertyBinder : IEntityMemberBinder<PropertyInfo>
    {
        public Expression Bind(PropertyInfo property, Expression bindingExpression)
        {
            throw new NotImplementedException();
        }
    }
}
