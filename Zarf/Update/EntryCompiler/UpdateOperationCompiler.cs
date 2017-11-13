using System.Collections.Generic;
using Zarf.Entities;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public class UpdateOperationCompiler : ModifyOperationCompiler
    {
        private IEntityTracker _tracker;

        public UpdateOperationCompiler(IEntityTracker tracker)
        {
            _tracker = tracker;
        }

        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor primary)
        {
            var updatedColumns = new List<string>();
            var paramemters = new List<DbParameter>();

            var isTracked = _tracker.IsTracked(entry.Entity);

            foreach (var item in entry.Members)
            {
                if (item.IsIncrement || item.IsPrimary || item.Member == primary.Member)
                {
                    continue;
                }

                var parameter = GetDbParameter(entry.Entity, item);
                if (isTracked && !_tracker.IsValueChanged(entry.Entity, item.Member, parameter.Value))
                {
                    continue;
                }

                updatedColumns.Add(GetColumnName(item));
                paramemters.Add(parameter);
            }

            return new DbUpdateCommand(entry, updatedColumns, paramemters, GetColumnName(primary), GetDbParameter(entry.Entity, primary));
        }
    }
}
