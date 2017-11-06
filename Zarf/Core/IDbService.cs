using System.Data;

namespace Zarf.Core
{
    public interface IDbService
    {
        void SetConnectionString(string connectionString);

        IDbCommand GetDbCommand();

        IDbCommand GetDbCommand(IDbConnection connection);

        IDbCommand GetDbCommand(IDbConnection connection, IDbTransaction transaction);

        IDbConnection GetDbConnection(string connectionString = "");

        IDbTransaction GetDbTransaction(IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Snapshot);
    }
}
