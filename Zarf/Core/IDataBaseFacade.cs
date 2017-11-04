using System.Data;

namespace Zarf.Core
{
    public interface IDataBaseFacade
    {
        IDbCommandFacade GetCommand();
    }
}
