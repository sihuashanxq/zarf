using System.Data;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IDbCommandFacade
    {
        TValue ExecuteScalar<TValue>(string commandText, params DbParameter[] dbParams);

        void ExecuteNonQuery(string commandText, params DbParameter[] dbParams);

        IDataReader ExecuteReader(string commandText, params DbParameter[] dbParams);
    }
}
