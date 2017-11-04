using Zarf.Update.Commands;

namespace Zarf.Update
{
    public interface IDbCommandExecutor<in TCommand>
        where TCommand : DbModifyCommand
    {
        int Execute(TCommand modifyCommand);
    }
}
