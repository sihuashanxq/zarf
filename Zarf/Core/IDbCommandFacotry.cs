using System.Data;

namespace Zarf.Core
{
    public interface IDbCommandFacotry
    {
        IDbCommandWrapper Create(IDbConnectionWrapper dbConnection);

        IDbCommandWrapper Create();

        IDbCommandWrapper Create(IDbConnection dbConnection);
    }
}
