using Zarf.Update.Commands;
using System.Collections.Generic;

namespace Zarf.Update
{
    public interface IDbCommandExecutor<TModifyCommand>
        where TModifyCommand : DbModifyCommand
    {
        int Execute(IEnumerable<EntityEntry> entries);
    }

    public interface IDbCommandExecutor : IDbCommandExecutor<DbModifyCommand>
    {

    }
}
