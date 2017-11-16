using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Zarf.Entities;

namespace Zarf.Update
{
    public class DbModificationCommandGroupBuilder
    {
        const int MaxParameterCount = 999;

        private int _colPostfix;

        private IEntityTracker _tracker;

        public DbModificationCommandGroupBuilder(IEntityTracker tracker)
        {
            _tracker = tracker;
        }

        public List<DbModificationCommandGroup> Build(IEnumerable<EntityEntry> entries)
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

        protected void BuildInsert(List<DbModificationCommandGroup> groups, EntityEntry entry)
        {
            var columns = new List<string>();
            var dbParams = new List<DbParameter>();

            foreach (var item in entry.Members)
            {
                if (item.IsAutoIncrement)
                {
                    continue;
                }

                columns.Add(GetColumnName(item));
                dbParams.Add(new DbParameter(GetNewParameterName(), item.GetValue(entry.Entity)));
            }

            AddCommandToGroup(groups, new DbModificationCommand(entry, columns, dbParams));
        }

        protected void BuildUpdate(List<DbModificationCommandGroup> groups, EntityEntry entry)
        {
            var columns = new List<string>();
            var paramemters = new List<DbParameter>();
            var isTracked = _tracker.IsTracked(entry.Entity);

            foreach (var item in entry.Members)
            {
                if (item.IsAutoIncrement || item.IsPrimary || entry.Primary == item)
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

        protected void BuildDelete(List<DbModificationCommandGroup> groups, EntityEntry entry)
        {
            AddCommandToGroup(
                groups,
                new DbModificationCommand(
                   entry,
                   GetColumnName(entry.Primary),
                   new List<DbParameter>() { GetDbParameter(entry.Entity, entry.Primary) })
              );
        }

        protected void AddCommandToGroup(List<DbModificationCommandGroup> groups, DbModificationCommand modifyCommand)
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
                last.DbParams.AddRange(modifyCommand.DbParams);
            }
            else
            {
                last.PrimaryKeyValues.AddRange(modifyCommand.PrimaryKeyValues);
            }
        }

        protected DbModificationCommandGroup FindCommadGroup(List<DbModificationCommandGroup> groups, DbModificationCommand modifyCommand)
        {
            var group = groups.LastOrDefault();
            if (group != null && group.DbParameterCount + modifyCommand.DbParameterCount < MaxParameterCount)
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

        protected string GetColumnName(MemberDescriptor memberDescriptor)
        {
            return memberDescriptor.Member.GetCustomAttribute<ColumnAttribute>()?.Name ?? memberDescriptor.Member.Name;
        }

        protected DbParameter GetDbParameter(object entity, MemberDescriptor memberDescriptor)
        {
            return new DbParameter(GetNewParameterName(), memberDescriptor.GetValue(entity));
        }
    }
}
