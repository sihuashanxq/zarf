using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Mapping;
using Zarf.Update;

namespace Zarf
{
    public class EntityEntry
    {
        public object Entity { get; set; }

        public Type Type { get; }

        public EntityState State { get; }

        public IEnumerable<MemberDescriptor> Members { get; }

        public MemberDescriptor Increment => Members?.FirstOrDefault(item => item.IsIncrement);

        public MemberDescriptor ConventionId => Members?.FirstOrDefault(item => item.Member.Name.ToLower() == "id");

        public MemberDescriptor Primary => Members?.FirstOrDefault(item => item.IsPrimary) ?? Increment ?? ConventionId;

        public EntityEntry(object entity, EntityState state, IEnumerable<MemberDescriptor> members)
        {
            Entity = entity;
            State = state;
            Members = members;
            Type = entity.GetType();
        }

        public static EntityEntry Create(object entity, EntityState state)
        {
            var memberDescriptors = new List<MemberDescriptor>();
            EntityTypeDescriptorFactory
                .Factory
                .Create(entity.GetType())
                .GetExpandMembers()
                .ToList()
                .ForEach(item => memberDescriptors.Add(new MemberDescriptor(item)));

            return new EntityEntry(entity, state, memberDescriptors);
        }
    }
}
