using Zarf.Update.Commands;

namespace Zarf.Update
{
    public interface IDbCommandExecutor<TModifyCommand>
        where TModifyCommand : DbModifyCommand
    {
        int Execute(DbModifyOperation modifyOperation);
    }

    public interface IDbCommandExecutor : IDbCommandExecutor<DbModifyCommand>
    {

    }
}
