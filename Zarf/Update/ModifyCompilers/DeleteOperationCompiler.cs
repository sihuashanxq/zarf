using Zarf.Entities;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public class DeleteOperationCompiler : ModifyOperationCompiler
    {
        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor identity)
        {
            return new DbDeleteCommand(
                entry,
                GetColumnName(identity),
                new DbParameter("@" + identity.Member.Name, identity.GetValue(entry.Entity))
            );
        }
    }
}
