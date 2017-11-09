using System.Data;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IDbCommandWrapper
    {
        IDbConnectionWrapper DbConnection { get; }

        IDbCommand DbCommand { get; }

        void BeginTransaction();

        void RollbackTransaction();

        void CommitTransaction();

        IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams);

        void ExecuteNonQuery(string commandText, params DbParameter[] dbParams);

        TValueType ExecuteScalar<TValueType>(string commandText, params DbParameter[] dbParams);

        void AddParameterWithValue(string parameterName, object value);
    }
}
