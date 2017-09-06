using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Zarf
{
    public interface IDbService
    {
        IDbTransaction CreateDbTransaction(IDbConnection dbConnection);

        IDbCommand CreateDbCommand();

        IDbConnection CreateDbConnection(string connectionString);
    }
}
