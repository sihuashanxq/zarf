using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Zarf.Metadata.Descriptors
{
    public class TypeDescriptorCacheFactory
    {
        private static ConcurrentDictionary<Type, TypeDescriptor> Caches { get; }

        public static TypeDescriptorCacheFactory Factory { get; } = new TypeDescriptorCacheFactory();

        static TypeDescriptorCacheFactory()
        {
            Caches = new ConcurrentDictionary<Type, TypeDescriptor>();
        }

        public TypeDescriptor Create(Type typeOfEntity)
        {
            return Caches.GetOrAdd(typeOfEntity, key =>
            {
                var members = new List<IMemberDescriptor>();
                var properties = typeOfEntity
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(item => Utils.IsAnonymouseType(typeOfEntity) || (item.CanRead && item.CanWrite))
                    .Where(item => Utils.IsAnonymouseType(typeOfEntity) || ReflectionUtil.SimpleTypes.Contains(item.PropertyType));

                var fileds = typeOfEntity
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(item => Utils.IsAnonymouseType(typeOfEntity) || ReflectionUtil.SimpleTypes.Contains(item.FieldType));

                foreach (var property in properties)
                {
                    members.Add(new MemberDescriptor(property));
                }

                foreach (var field in fileds)
                {
                    members.Add(new MemberDescriptor(field));
                }

                return new TypeDescriptor(typeOfEntity, members);
            });
        }
    }
}
