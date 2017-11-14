using System.Collections.Generic;
using Zarf.Entities;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public class DeleteOperationCompiler : ModifyOperationCompiler
    {
        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor primary)
        {
            return new DbDeleteCommand(
                entry,
                GetColumnName(primary),
                new List<DbParameter>() { new DbParameter("@" + primary.Member.Name, primary.GetValue(entry.Entity)) }
            );
        }
    }
}
