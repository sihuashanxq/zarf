using System.Data;

namespace Zarf.Core
{
    public interface IDataBaseFacade
    {
        IDbCommandFacade GetCommand();
    }

    public interface IDbCommandFacotry
    {
        IDbCommand Create(IDbConnection dbConnection);
    }

    public interface IDbConnectionFacotry
    {
        IDbConnection Create();
    }
}
