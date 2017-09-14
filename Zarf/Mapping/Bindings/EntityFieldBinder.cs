using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping.Activators
{
    public class EntityFieldBinder : IEntityMemberBinder<FieldInfo>
    {
        public void Bind(FieldInfo member, Expression bindExpression)
        {
            throw new NotImplementedException();
        }
    }
}
