using System.Data;
using System;

namespace Zarf.Core
{
    public interface IDbEntityConnection : IDisposable
    {
        IDbTransaction DbTransaction { get; }

        IDbConnection DbConnection { get; }

        string ConnectionString { get; }

        IDbEntityTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);

        void Open();

        void Close();
    }
}
