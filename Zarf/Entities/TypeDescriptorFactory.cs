using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zarf.Update;
using System.Collections.Concurrent;
using Zarf.Entities;

namespace Zarf.Mapping
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
                foreach (var property in typeOfEntity
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(item => item.CanRead && item.CanWrite)
                    .Where(item => ReflectionUtil.SimpleTypes.Contains(item.PropertyType)))
                {
                    members.Add(new MemberDescriptor(property));
                }

                foreach (var field in typeOfEntity
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(item => ReflectionUtil.SimpleTypes.Contains(item.FieldType)))
                {
                    members.Add(new MemberDescriptor(field));
                }

                return new TypeDescriptor(typeOfEntity, members);
            });
        }
    }
}
