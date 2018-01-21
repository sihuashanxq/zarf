using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zarf.Metadata.Descriptors
{
    public class TypeDescriptor
    {
        public Type Type { get; }

        public List<IMemberDescriptor> MemberDescriptors { get; }

        public ConstructorInfo Constructor { get; }

        public TypeDescriptor(Type typeOfEntity, List<IMemberDescriptor> memberDescriptors)
        {
            Type = typeOfEntity;
            MemberDescriptors = memberDescriptors;
            Constructor = typeOfEntity.GetConstructor(Type.EmptyTypes) ?? typeOfEntity.GetConstructors()[0];
        }
    }
}
