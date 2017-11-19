using System.Data;
using System.Threading.Tasks;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IDbEntityCommand
    {
        IDbEntityConnection EntityConnection { get; }

        IDbCommand DbCommand { get; }

        IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams);

        void ExecuteNonQuery(string commandText, params DbParameter[] dbParams);

        object ExecuteScalar(string commandText, params DbParameter[] dbParams);

        Task<IDataReader> ExecuteDataReaderAsync(string commandText, params DbParameter[] dbParams);

        Task ExecuteNonQueryAsync(string commandText, params DbParameter[] dbParams);

        Task<object> ExecuteScalarAsync(string commandText, params DbParameter[] dbParams);

        void AddParameterWithValue(string parameterName, object value);
    }
}
