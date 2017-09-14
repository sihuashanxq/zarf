using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping
{
    public class EntityMemberValueProvider : IEntityMemberValueProvider
    {
        public IEntityProjectionMapping _mapping;

        public static ParameterExpression ActivateMethodParameter { get; }

        public static MethodInfo ActivateMethod { get; }

        static EntityMemberValueProvider()
        {
            ActivateMethodParameter = Expression.Parameter(typeof(IDataReader), "dbDataReader");
            ActivateMethod = typeof(EntityMemberValueProvider).GetMethod(nameof(GetValue));
        }

        public EntityMemberValueProvider(IEntityProjectionMapping mapping)
        {
            _mapping = mapping;
        }

        public object GetValue(IDataReader dbDataReader)
        {
            if (dbDataReader.IsDBNull(_mapping.Ordinal))
            {
                return null;
            }

            return dbDataReader.GetValue(_mapping.Ordinal);
        }

        public static IEntityMemberValueProvider CreateProvider(IEntityProjectionMapping mapping)
        {
            return new EntityMemberValueProvider(mapping);
        }
    }
}
