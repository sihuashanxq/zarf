using System.Data;
using Zarf.Update.Executors;

namespace Zarf.Core
{
    public interface IDataBaseFacade
    {
        IDbConnection GetConnection();

        IDbCommandFacade GetCommand();
    }
}
