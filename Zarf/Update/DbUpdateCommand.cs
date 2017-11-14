using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;

namespace Zarf.Update.Commands
{
    public class DbUpdateCommand : DbModifyCommand
    {
        public DbUpdateCommand(
            EntityEntry entry,
            IEnumerable<string> updatedColumns,
            IEnumerable<DbParameter> updatedColumnParams,
            string primaryKey,
            DbParameter primaryKeyValue)
            : base(entry)
        {
            Columns = updatedColumns.ToList();
            DbParams = updatedColumnParams.ToList();
            PrimaryKey = primaryKey;
            PrimaryKeyValues = new List<DbParameter>() { primaryKeyValue };
        }
    }
}
