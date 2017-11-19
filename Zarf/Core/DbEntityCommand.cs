using System;
using System.Data;
using System.Threading.Tasks;
using Zarf.Entities;

namespace Zarf.Core
{
    public abstract class DbEntityCommand : IDbEntityCommand
    {
        public IDbEntityConnection EntityConnection { get; }

        public IDbCommand DbCommand { get; }

        public DbEntityCommand(IDbCommand dbCommand, IDbEntityConnection dbConnection)
        {
            DbCommand = dbCommand;
            EntityConnection = dbConnection;
        }

        public IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams)
        {
            return PrepareDbCommand(commandText, dbParams).ExecuteReader(CommandBehavior.Default);
        }

        public object ExecuteScalar(string commandText, params DbParameter[] dbParams)
        {
            return PrepareDbCommand(commandText, dbParams).ExecuteScalar();
        }

        public void ExecuteNonQuery(string commandText, params DbParameter[] dbParams)
        {
            PrepareDbCommand(commandText, dbParams).ExecuteNonQuery();
        }

        public void AddParameterWithValue(string parameterName, object value)
        {
            var parameter = DbCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            DbCommand.Parameters.Add(parameter);
        }

        protected virtual IDbCommand PrepareDbCommand(string commandText, params DbParameter[] dbParams)
        {
            DbCommand.Parameters.Clear();
            DbCommand.CommandText = commandText;
            if (DbCommand.Connection.State == ConnectionState.Closed)
            {
                DbCommand.Connection.Open();
            }

            if (dbParams != null)
            {
                foreach (var item in dbParams)
                {
                    AddParameterWithValue(item.Name, item.Value);
                }
            }

            return DbCommand;
        }

        public abstract Task<IDataReader> ExecuteDataReaderAsync(string commandText, params DbParameter[] dbParams);

        public abstract Task ExecuteNonQueryAsync(string commandText, params DbParameter[] dbParams);

        public abstract Task<object> ExecuteScalarAsync(string commandText, params DbParameter[] dbParams);
    }
}