using System;
using System.Collections.Generic;
using Zarf.Update;
using System.Linq.Expressions;
using Zarf.Entities;
using System.Linq;
using Zarf.Extensions;

namespace Zarf
{
    public class EntityEntry
    {
        public Type Type { get; }

        public object Entity { get; }

        public EntityState State { get; }

        public IEnumerable<MemberDescriptor> Members { get; }

        public MemberDescriptor IncrementMember => Members?.FirstOrDefault(item => item.IsIncrement);

        public MemberDescriptor PrimaryMember => Members?.FirstOrDefault(item => item.IsPrimary);
    }

    public class DbModifyOperation
    {
        public IEnumerable<EntityEntry> Entities { get; }

        public LambdaExpression Predicate { get; }
    }

    public class DbModifyCommand
    {
        public Table Table { get; }

        public IEnumerable<Column> Columns { get; }

        public LambdaExpression Predicate { get; }

        public IEnumerable<DbParameter> Parameters { get; }

        public EntityEntry Entity { get; }

        public DbModifyCommand(
            EntityEntry entity,
            IEnumerable<Column> columns,
            IEnumerable<DbParameter> dbParams,
            LambdaExpression predicate)
        {
            Entity = entity;
            Table = Entity.Type.ToTable();
            Columns = columns;
            Parameters = dbParams;
            Predicate = predicate;
        }
    }

    public enum EntityState
    {
        Add,
        Update,
        Delete
    }
}
