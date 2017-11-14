using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;

namespace Zarf.Update.Commands
{
    public class DbInsertCommand : DbModifyCommand
    {
        public DbInsertCommand(
            EntityEntry entity,
            IEnumerable<string> columns,
            IEnumerable<DbParameter> dbParams)
            : base(entity)
        {
            Columns = columns.ToList();
            DbParams = dbParams.ToList();
        }
    }
}
