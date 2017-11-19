using System.Data;
using System.Threading.Tasks;
using Zarf.Core;
using Zarf.Entities;
using System.Data.SqlClient;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbEntityCommand : DbEntityCommand
    {
        public SqlCommand SqlCommand => DbCommand as SqlCommand;

        public SqlServerDbEntityCommand(IDbCommand dbCommand, IDbEntityConnection dbConnection) : base(dbCommand, dbConnection)
        {
            DbCommand.Connection = EntityConnection.DbConnection;
            DbCommand.Transaction = EntityConnection.DbTransaction;
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
