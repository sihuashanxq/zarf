using System.Data;
using System.Threading.Tasks;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IDbEntityCommand
    {
        IDbEntityConnection EntityConnection { get; }

        IDbCommand DbCommand { get; }

        IDataReader ExecuteDataReader(string commandText, params DbParameter[] parameters);

        void ExecuteNonQuery(string commandText, params DbParameter[] parameters);

        object ExecuteScalar(string commandText, params DbParameter[] parameters);

        Task<IDataReader> ExecuteDataReaderAsync(string commandText, params DbParameter[] parameters);

        Task ExecuteNonQueryAsync(string commandText, params DbParameter[] parameters);

        Task<object> ExecuteScalarAsync(string commandText, params DbParameter[] parameters);

        void AddParameterWithValue(string parameterName, object value);
    }
}
