using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Linq;

namespace Zarf.Mapping
{
    public class EntityTypeDescriptorFactory
    {
        private static readonly Dictionary<Type, EntityTypeDescriptor> _entityTypeDescriptors;

        public static EntityTypeDescriptorFactory Factory { get; }

        static EntityTypeDescriptorFactory()
        {
            _entityTypeDescriptors = new Dictionary<Type, EntityTypeDescriptor>();
            Factory = new EntityTypeDescriptorFactory();
        }

        public EntityTypeDescriptor Create(Type entityType)
        {
            if (_entityTypeDescriptors.TryGetValue(entityType, out EntityTypeDescriptor entityTypeDescriptor))
            {
                return entityTypeDescriptor;
            }

            entityTypeDescriptor = new EntityTypeDescriptor(entityType);

            foreach (var property in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (ReflectionUtil.SimpleTypes.Contains(property.PropertyType))
                {
                    entityTypeDescriptor.Members.Add(property);
                }
                else if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var propertyTypeDescriptor = Create(property.PropertyType);
                    entityTypeDescriptor.Members.AddRange(propertyTypeDescriptor.Members);
                }
            }

            foreach (var field in entityType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (ReflectionUtil.SimpleTypes.Contains(field.FieldType))
                {
                    entityTypeDescriptor.Members.Add(field);
                }
                else if (!typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                {
                    var fieldTypeDescriptor = Create(field.FieldType);
                    entityTypeDescriptor.Members.AddRange(fieldTypeDescriptor.Members);
                }
            }

            return _entityTypeDescriptors[entityType] = entityTypeDescriptor;
        }
    }
}
