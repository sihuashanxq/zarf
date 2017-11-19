using System.Data;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IDbEntityCommand
    {
        IDbEntityConnection EntityConnection { get; }

        IDbCommand DbCommand { get; }

        IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams);

        void ExecuteNonQuery(string commandText, params DbParameter[] dbParams);

        TValueType ExecuteScalar<TValueType>(string commandText, params DbParameter[] dbParams);

        void AddParameterWithValue(string parameterName, object value);
    }
}
