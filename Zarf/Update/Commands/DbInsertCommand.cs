using System.Collections.Generic;
using Zarf.Entities;

namespace Zarf.Update.Commands
{
    public class DbInsertCommand : DbModifyCommand
    {
        public IEnumerable<string> Columns { get; }

        public IEnumerable<DbParameter> DbParams { get; }

        public DbInsertCommand(
            EntityEntry entity,
            IEnumerable<string> columns,
            IEnumerable<DbParameter> dbParams)
            : base(entity)
        {
            Columns = columns;
            DbParams = dbParams;
        }
    }
}
