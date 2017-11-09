using System;
using System.Collections.Generic;
using Zarf.Entities;

namespace Zarf.Update.Commands
{
    public class DbUpdateCommand : DbModifyCommand
    {
        public IEnumerable<string> Columns { get; }

        public IEnumerable<DbParameter> DbParams { get; }

        public string IdentityColumn { get; }

        public DbParameter IdentityColumnValue { get; }

        public DbUpdateCommand(
            EntityEntry entry,
            IEnumerable<string> updatedColumns,
            IEnumerable<DbParameter> updatedColumnParams,
            string identityColumn,
            DbParameter identityColumnValue)
            : base(entry)
        {
            Columns = updatedColumns;
            DbParams = updatedColumnParams;
            IdentityColumn = identityColumn;
            IdentityColumnValue = identityColumnValue;
        }
    }
}
