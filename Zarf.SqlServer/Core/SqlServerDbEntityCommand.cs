using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Zarf.Core;
using Zarf.Entities;

namespace Zarf.SqlServer.Core
{
    internal class SqlServerDbEntityCommand : DbEntityCommand
    {
        internal SqlCommand SqlCommand => DbCommand as SqlCommand;

        internal SqlServerDbEntityConnection SqlEntityConnection => EntityConnection as SqlServerDbEntityConnection;

        internal SqlServerDbEntityCommand(IDbCommand dbCommand, IDbEntityConnection dbConnection) : base(dbCommand, dbConnection)
        {
            SqlCommand.Connection = SqlEntityConnection.SqlConnection;
            SqlCommand.Transaction = SqlEntityConnection.SqlTransaction;
        }

        public override async Task<IDataReader> ExecuteDataReaderAsync(string commandText, params DbParameter[] dbParams)
        {
            PrepareDbCommand(commandText, dbParams);
            return await SqlCommand.ExecuteReaderAsync();
        }

        public override async Task ExecuteNonQueryAsync(string commandText, params DbParameter[] dbParams)
        {
            PrepareDbCommand(commandText, dbParams);
            await SqlCommand.ExecuteNonQueryAsync();
        }

        public override async Task<object> ExecuteScalarAsync(string commandText, params DbParameter[] dbParams)
        {
            PrepareDbCommand(commandText, dbParams);
            return await SqlCommand.ExecuteScalarAsync();
        }
    }
}
