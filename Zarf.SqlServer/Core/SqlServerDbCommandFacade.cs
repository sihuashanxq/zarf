using System;
using System.Data;
using Zarf.Core;
using Zarf.Entities;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbCommandFacade : IDbCommandFacade
    {
        public IDbService DbService { get; }

        public SqlServerDbCommandFacade(IDbService dbService)
        {
            DbService = dbService;
        }

        public void ExecuteNonQuery(string commandText, params DbParameter[] dbParams)
        {
            PrepareDbCommand(commandText, dbParams).ExecuteNonQuery();
        }

        public IDataReader ExecuteReader(string commandText, params DbParameter[] dbParams)
        {
            return PrepareDbCommand(commandText, dbParams).ExecuteReader(CommandBehavior.Default);
        }

        public TValue ExecuteScalar<TValue>(string commandText, params DbParameter[] dbParams)
        {
            var rV = PrepareDbCommand(commandText, dbParams).ExecuteScalar();
            return (TValue)Convert.ChangeType(rV, typeof(TValue));
        }

        private IDbCommand PrepareDbCommand(string commandText, params DbParameter[] dbParams)
        {
            var dbCommand = DbService.GetDbCommand();
            dbCommand.CommandText = commandText;

            if (dbCommand.Connection.State == ConnectionState.Closed)
            {
                dbCommand.Connection.Open();
            }

            if (dbParams == null || dbParams.Length == 0)
            {
                return dbCommand;
            }

            foreach (var item in dbParams)
            {
                var dbParameter = dbCommand.CreateParameter();

                dbParameter.ParameterName = item.Name;
                dbParameter.Value = item.Value;

                dbCommand.Parameters.Add(dbParameter);
            }

            return dbCommand;
        }
    }
}
