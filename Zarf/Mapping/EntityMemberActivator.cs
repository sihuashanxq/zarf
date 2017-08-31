using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping
{
    public class EntityMemberActivator : IObjectActivator
    {
        public IMapping _mapping;

        public static ParameterExpression ActivateMethodParameter { get; }

        public static MethodInfo ActivateMethod { get; }

        static EntityMemberActivator()
        {
            ActivateMethodParameter = Expression.Parameter(typeof(IDataReader), "dbDataReader");
            ActivateMethod = typeof(EntityMemberActivator).GetMethod(nameof(CreateInstance));
        }

        public EntityMemberActivator(IMapping mapping)
        {
            _mapping = mapping;
        }

        public object CreateInstance(IDataReader dbDataReader)
        {
            if (dbDataReader.IsDBNull(_mapping.Ordinal))
            {
                return null;
            }

            return dbDataReader.GetValue(_mapping.Ordinal);
        }

        public static IObjectActivator CreateActivator(IMapping mapping)
        {
            return new EntityMemberActivator(mapping);
        }
    }
}
