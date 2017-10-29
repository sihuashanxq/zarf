using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace Zarf.Mapping
{
    public class EntityTypeDescriptorFactory
    {
        private static readonly Dictionary<Type, EntityTypeDescriptor> _typeDescriptors;

        public static EntityTypeDescriptorFactory Factory { get; }

        static EntityTypeDescriptorFactory()
        {
            _typeDescriptors = new Dictionary<Type, EntityTypeDescriptor>();
            Factory = new EntityTypeDescriptorFactory();
        }

        public EntityTypeDescriptor Create(Type entityType)
        {
            if (_typeDescriptors.TryGetValue(entityType, out EntityTypeDescriptor typeDescriptor))
            {
                return typeDescriptor;
            }

            typeDescriptor = new EntityTypeDescriptor(entityType);

            foreach (var property in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (ReflectionUtil.SimpleTypes.Contains(property.PropertyType))
                {
                    typeDescriptor.Members.Add(property);
                }
                else if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var propertyTypeDescriptor = Create(property.PropertyType);
                    typeDescriptor.Members.AddRange(propertyTypeDescriptor.Members);
                }
            }

            foreach (var field in entityType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (ReflectionUtil.SimpleTypes.Contains(field.FieldType))
                {
                    typeDescriptor.Members.Add(field);
                }
                else if (!typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                {
                    var fieldTypeDescriptor = Create(field.FieldType);
                    typeDescriptor.Members.AddRange(fieldTypeDescriptor.Members);
                }
            }

            return _typeDescriptors[entityType] = typeDescriptor;
        }
    }
}
