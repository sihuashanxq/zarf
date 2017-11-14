using System.Collections.Generic;
using Zarf.Entities;

namespace Zarf.Update.Commands
{
    public class DbDeleteCommand : DbModifyCommand
    {
        public DbDeleteCommand(EntityEntry entity, string primaryKey, List<DbParameter> primaryKeyValues)
            : base(entity)
        {
            PrimaryKey = primaryKey;
            PrimaryKeyValues = primaryKeyValues;
        }
    }
}
