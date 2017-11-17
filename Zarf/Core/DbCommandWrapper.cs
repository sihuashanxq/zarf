using System;
using System.Data;
using Zarf.Entities;

namespace Zarf.Core
{
    public class DbCommandWrapper : IDbCommandWrapper
    {
        public IDbConnectionWrapper DbConnection { get; }

        public IDbCommand DbCommand { get; }

        public DbCommandWrapper(IDbCommand dbCommand, IDbConnectionWrapper dbConnection)
        {
            DbCommand = dbCommand;
            DbConnection = dbConnection;
        }

        public IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams)
        {
            return PrepareDbCommand(commandText, dbParams).ExecuteReader(CommandBehavior.Default);
        }

        public TValueType ExecuteScalar<TValueType>(string commandText, params DbParameter[] dbParams)
        {
            var rV = PrepareDbCommand(commandText, dbParams).ExecuteScalar();
            return (TValueType)Convert.ChangeType(rV, typeof(TValueType));
        }

        public void ExecuteNonQuery(string commandText, params DbParameter[] dbParams)
        {
            PrepareDbCommand(commandText, dbParams).ExecuteNonQuery();
        }

        public void BeginTransaction()
        {
            DbCommand.Transaction = DbCommand.Connection.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            DbCommand.Transaction?.Rollback();
        }

        public void CommitTransaction()
        {
            DbCommand.Transaction?.Commit();
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

            if (dbParams != null)
            {
                foreach (var item in dbParams)
                {
                    AddParameterWithValue(item.Name, item.Value);
                }
            }

            DbCommand.CommandText = commandText;
            DbCommand.Connection = DbConnection.DbConnection;
            DbCommand.Transaction = DbConnection.CurrentTransaction;

            if (DbConnection.DbConnection.State == ConnectionState.Closed)
            {
                DbConnection.DbConnection.Open();
            }

            return DbCommand;
        }
    }
}
