using System;
using System.Data;
using Zarf.Entities;

namespace Zarf.Core
{
    public class DbEntityCommand : IDbEntityCommand
    {
        public IDbEntityConnection EntityConnection { get; }

        public IDbCommand DbCommand { get; }

        public DbEntityCommand(IDbCommand dbCommand, IDbEntityConnection dbConnection)
        {
            DbCommand = dbCommand;
            EntityConnection = dbConnection;
            DbCommand.Connection = EntityConnection.DbConnection;
            DbCommand.Transaction = EntityConnection.DbTransaction;
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
    }
}
