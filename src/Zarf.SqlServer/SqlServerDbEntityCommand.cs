using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Zarf.Core;
using Zarf.Metadata.Entities;

namespace Zarf.SqlServer
{
    internal class SqlServerDbEntityCommand : DbEntityCommand
    {
        internal SqlCommand Command => DbCommand as SqlCommand;

        internal SqlServerDbEntityConnection SqlEntityConnection => EntityConnection as SqlServerDbEntityConnection;

        internal SqlServerDbEntityCommand(IDbCommand dbCommand, IDbEntityConnection dbConnection) : base(dbCommand, dbConnection)
        {
            Command.Connection = SqlEntityConnection.Connection;
            Command.Transaction = SqlEntityConnection.Transaction;
        }

        public override async Task<IDataReader> ExecuteDataReaderAsync(string commandText, params DbParameter[] parameters)
        {
            PrepareDbCommand(commandText, parameters);
            return await Command.ExecuteReaderAsync();
        }

        public override async Task ExecuteNonQueryAsync(string commandText, params DbParameter[] parameters)
        {
            PrepareDbCommand(commandText, parameters);
            await Command.ExecuteNonQueryAsync();
        }

        public override async Task<object> ExecuteScalarAsync(string commandText, params DbParameter[] parameters)
        {
            PrepareDbCommand(commandText, parameters);
            return await Command.ExecuteScalarAsync();
        }
    }
}
