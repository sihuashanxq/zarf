using System.Data;

namespace Zarf.Core
{
    public interface IDbConnectionWrapper
    {
        IDbTransaction CurrentTransaction { get; }

        IDbConnection DbConnection { get; }

        string ConnectionString { get; }

        IDbTransactionWrapper BeginTransaction();

        void RollbackTransaction();

        void CommitTransaction();
    }
}
