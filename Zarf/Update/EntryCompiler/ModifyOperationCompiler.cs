using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Zarf.Entities;
using Zarf.Update.Commands;
using System.Linq;

namespace Zarf.Update.Compilers
{
    public abstract class ModifyOperationCompiler : IModifyOperationCompiler
    {
        public virtual IEnumerable<DbModifyCommand> Compile(IEnumerable<EntityEntry> entries)
        {
            foreach (var entry in entries.OrderBy(item => item.State).ThenByDescending(item => item.Entity.GetType().GetHashCode()))
            {
                var modifyCommand = Compile(entry, entry.Primary);
                if (modifyCommand != null)
                {
                    yield return modifyCommand;
                }
            }
        }

        public abstract DbModifyCommand Compile(EntityEntry entry, MemberDescriptor primary);

        protected string GetColumnName(MemberDescriptor memberDescriptor)
        {
            return memberDescriptor.Member.GetCustomAttribute<ColumnAttribute>()?.Name ?? memberDescriptor.Member.Name;
        }

        protected DbParameter GetDbParameter(object entity, MemberDescriptor memberDescriptor)
        {
            return new DbParameter("@" + GetColumnName(memberDescriptor), memberDescriptor.GetValue(entity));
        }
    }
}
