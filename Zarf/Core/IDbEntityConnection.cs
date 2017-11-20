using System.Data;
using System;

namespace Zarf.Core
{
    public interface IDbEntityConnection : IDisposable
    {
        IDbConnection DbConnection { get; }

        string ConnectionString { get; }

        IDbEntityTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);

        bool HasTransaction();

        void Open();

        void Close();
    }
}
