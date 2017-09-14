using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping.Activators
{
    public class EntityFieldBinder : IEntityMemberBinder<FieldInfo>
    {
        public Expression Bind(FieldInfo fiedInfo, Expression bindingExpression)
        {
            throw new NotImplementedException();
        }
    }
}
