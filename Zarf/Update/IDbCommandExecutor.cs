using Zarf.Update.Commands;

namespace Zarf.Update
{
    public interface IDbCommandExecutor
    {
        int Execute(DbModifyCommand modifyCommand);
    }
}
