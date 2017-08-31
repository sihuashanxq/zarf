using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
namespace Zarf.Core
{
    public interface IDbService
    {
        IDbTransaction DbTransaction { get; }

        IDbConnection DbConnection { get; }

        IDbCommand DbCommand { get; }
    }
}
