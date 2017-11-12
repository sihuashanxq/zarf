using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Zarf.Entities;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public abstract class ModifyOperationCompiler : IModifyOperationCompiler
    {
        public virtual IEnumerable<DbModifyCommand> Compile(DbModifyOperation modifyOperation)
        {
            foreach (var entry in modifyOperation.Entries)
            {
                var modifyCommand = Compile(entry, modifyOperation.Identity);
                if (modifyCommand != null)
                {
                    yield return modifyCommand;
                }
            }
        }

        public abstract DbModifyCommand Compile(EntityEntry entry, MemberDescriptor identity);

        public virtual void TrackEntity<TEntity>(TEntity entity)
        {

        }

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
