using System.Collections.Generic;
using Zarf.Update.Commands;

namespace Zarf.Update
{
    public interface IModifyOperationCompiler
    {
        IEnumerable<DbModifyCommand> Compile(IEnumerable<EntityEntry> entries);

        DbModifyCommand Compile(EntityEntry entry, MemberDescriptor primary);
    }
}
