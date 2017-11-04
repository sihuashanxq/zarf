using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Mapping;
using Zarf.Update;

namespace Zarf
{
    public class EntityEntry
    {
        public Type Type { get; }

        public object Entity { get; }

        public EntityState State { get; }

        public IEnumerable<MemberDescriptor> Members { get; }

        public MemberDescriptor Increment => Members?.FirstOrDefault(item => item.IsIncrement);

        public MemberDescriptor Primary => Members?.FirstOrDefault(item => item.IsPrimary);

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

    public class DbModifyOperation
    {
        public IEnumerable<EntityEntry> Entries { get; }

        public MemberDescriptor Identity { get; }

        public DbModifyOperation(IEnumerable<EntityEntry> entries, MemberDescriptor identity)
        {
            Entries = entries;
            Identity = identity;
        }
    }

    public enum EntityState
    {
        Insert,
        Update,
        Delete
    }
}
