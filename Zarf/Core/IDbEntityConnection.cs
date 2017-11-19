using System.Data;

namespace Zarf.Core
{
    public interface IDbEntityConnection
    {
        IDbTransaction DbTransaction { get; }

        IDbConnection DbConnection { get; }

        string ConnectionString { get; }

        IDbEntityTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);

        void Open();

        void Close();
    }
}
