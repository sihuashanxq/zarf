using Zarf.Entities;

namespace Zarf.Update.Commands
{
    public class DbDeleteCommand : DbModifyCommand
    {
        public string IdentityColumn { get; }

        public DbParameter IdentityColumnValue { get; }

        public DbDeleteCommand(EntityEntry entity, string identityColumn, DbParameter identityColumnValue)
            : base(entity)
        {
            IdentityColumn = identityColumn;
            IdentityColumnValue = identityColumnValue;
        }
    }
}
