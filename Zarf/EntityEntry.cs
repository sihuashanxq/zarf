using System;
using System.Collections.Generic;
using Zarf.Update;
using Zarf.Entities;
using System.Linq;
using Zarf.Extensions;

namespace Zarf
{
    /*
    如果说有byKey
    那么就解析一个Member
    把这个Member作为这次的PrimaryMember
    */
    public class EntityEntry
    {
        public Type Type { get; }

        public object Entity { get; }

        public EntityState State { get; }

        public IEnumerable<MemberDescriptor> Members { get; }

        public MemberDescriptor Increment => Members?.FirstOrDefault(item => item.IsIncrement);

        public MemberDescriptor Primary => Members?.FirstOrDefault(item => item.IsPrimary);
    }

    public class DbModifyOperation
    {
        public IEnumerable<EntityEntry> Entities { get; }

        public MemberDescriptor OperationKey { get; }
    }

    public class DbModifyCommand
    {
        public Table Table { get; }

        public IEnumerable<Column> Columns { get; }

        public IEnumerable<DbParameter> Parameters { get; }

        public EntityEntry Entity { get; }

        public DbModifyCommand(
            EntityEntry entity,
            IEnumerable<Column> columns,
            IEnumerable<DbParameter> dbParams)
        {
            Entity = entity;
            Table = Entity.Type.ToTable();
            Columns = columns;
            Parameters = dbParams;
        }
    }

    public enum EntityState
    {
        Add,
        Update,
        Delete
    }
}
