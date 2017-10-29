using System.Data;

namespace Zarf.Core
{
    public interface IDbService
    {
        IDbCommand CreateDbCommand();

        IDbCommand CreateDbCommand(IDbConnection connection);

        IDbCommand CreateDbCommand(IDbConnection connection, IDbTransaction transaction);

        IDbConnection CreateDbConnection(string connectionString);

        IDbTransaction CreateDbTransaction(IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Snapshot);
    }
}
