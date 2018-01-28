using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Zarf.Metadata.Descriptors;
using Zarf.Metadata.Entities;

namespace Zarf.Update
{
    public class DbModificationCommandGroupBuilder : IDbModificationCommandGroupBuilder
    {
        protected virtual int MaxParameterCount => 1999;

        private int _colPostfix;

        private IEntityTracker _tracker;

        public DbModificationCommandGroupBuilder(IEntityTracker tracker)
        {
            _tracker = tracker;
        }

        public virtual List<DbModificationCommandGroup> Build(IEnumerable<EntityEntry> entries)
        {
            var groups = new List<DbModificationCommandGroup>();
            foreach (var item in entries.OrderBy(item => item.State).ThenBy(item => item.Type.GetHashCode()))
            {
                switch (item.State)
                {
                    case EntityState.Insert:
                        BuildInsert(groups, item);
                        break;
                    case EntityState.Update:
                        BuildUpdate(groups, item);
                        break;
                    default:
                        BuildDelete(groups, item);
                        break;
                }
            }

            return groups;
        }

        protected virtual void BuildInsert(List<DbModificationCommandGroup> groups, EntityEntry entry)
        {
            var columns = new List<string>();
            var parameters = new List<DbParameter>();

            foreach (var item in entry.Members)
            {
                if (item.IsAutoIncrement)
                {
                    continue;
                }

                columns.Add(GetColumnName(item));
                parameters.Add(new DbParameter(GetNewParameterName(), item.GetValue(entry.Entity)));
            }

            AddCommandToGroup(groups, new DbModificationCommand(entry, columns, parameters));
        }

        protected virtual void BuildUpdate(List<DbModificationCommandGroup> groups, EntityEntry entry)
        {
            var columns = new List<string>();
            var paramemters = new List<DbParameter>();
            var isTracked = _tracker.IsTracked(entry.Entity);

            foreach (var item in entry.Members)
            {
                if (item.IsAutoIncrement || item.IsPrimaryKey || entry.Primary == item)
                {
                    continue;
                }

                var parameter = new DbParameter(GetNewParameterName(), item.GetValue(entry.Entity));
                if (isTracked && !_tracker.IsValueChanged(entry.Entity, item.Member, parameter.Value))
                {
                    continue;
                }

                columns.Add(GetColumnName(item));
                paramemters.Add(parameter);
            }

            AddCommandToGroup(
                groups,
                new DbModificationCommand(
                    entry,
                    columns,
                    paramemters,
                    GetColumnName(entry.Primary),
                    GetDbParameter(entry.Entity, entry.Primary))
            );
        }

        protected virtual void BuildDelete(List<DbModificationCommandGroup> groups, EntityEntry entry)
        {
            AddCommandToGroup(
                groups,
                new DbModificationCommand(
                   entry,
                   GetColumnName(entry.Primary),
                   new List<DbParameter>() { GetDbParameter(entry.Entity, entry.Primary) })
              );
        }

        protected virtual void AddCommandToGroup(List<DbModificationCommandGroup> groups, DbModificationCommand modifyCommand)
        {
            var group = FindCommadGroup(groups, modifyCommand);
            if (group == null)
            {
                group = new DbModificationCommandGroup();
                groups.Add(group);
            }

            if (modifyCommand.State == EntityState.Update)
            {
                group.Commands.Add(modifyCommand);
                return;
            }

            var last = group.Commands.LastOrDefault();
            if (last == null ||
                last.Entry.State != modifyCommand.Entry.State ||
                last.Entry.Type != modifyCommand.Entry.Type)
            {
                group.Commands.Add(modifyCommand);
                return;
            }

            if (modifyCommand.State == EntityState.Insert)
            {
                last.Parameters.AddRange(modifyCommand.Parameters);
            }
            else
            {
                last.PrimaryKeyValues.AddRange(modifyCommand.PrimaryKeyValues);
            }
        }

        protected virtual DbModificationCommandGroup FindCommadGroup(List<DbModificationCommandGroup> groups, DbModificationCommand modifyCommand)
        {
            var group = groups.LastOrDefault();
            if (group != null && group.ParameterCount + modifyCommand.ParameterCount < MaxParameterCount)
            {
                if ((modifyCommand.State != EntityState.Insert || modifyCommand.Entry.AutoIncrementProperty == null) &&
                    group.Commands.Any(item => item.State != EntityState.Insert || item.Entry.AutoIncrementProperty == null))
                {
                    return group;
                }
            }

            return null;
        }

        protected string GetNewParameterName()
        {
            return "@P" + (_colPostfix++).ToString();
        }

        protected string GetColumnName(IMemberDescriptor memberDescriptor)
        {
            return memberDescriptor.Member.GetCustomAttribute<ColumnAttribute>()?.Name ?? memberDescriptor.Member.Name;
        }

        protected DbParameter GetDbParameter(object entity, IMemberDescriptor memberDescriptor)
        {
            return new DbParameter(GetNewParameterName(), memberDescriptor.GetValue(entity));
        }
    }
}
