using System.Data;
using System.Threading.Tasks;

using Zarf.Metadata.Entities;

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

        public IDataReader ExecuteDataReader(string commandText, params DbParameter[] parameters)
        {
            return PrepareDbCommand(commandText, parameters).ExecuteReader(CommandBehavior.Default);
        }

        public object ExecuteScalar(string commandText, params DbParameter[] parameters)
        {
            return PrepareDbCommand(commandText, parameters).ExecuteScalar();
        }

        public void ExecuteNonQuery(string commandText, params DbParameter[] parameters)
        {
            PrepareDbCommand(commandText, parameters).ExecuteNonQuery();
        }

        public void AddParameterWithValue(string parameterName, object value)
        {
            var parameter = DbCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            DbCommand.Parameters.Add(parameter);
        }

        protected virtual IDbCommand PrepareDbCommand(string commandText, params DbParameter[] parameters)
        {
            DbCommand.Parameters.Clear();
            DbCommand.CommandText = commandText;
            if (DbCommand.Connection.State == ConnectionState.Closed)
            {
                DbCommand.Connection.Open();
            }

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    AddParameterWithValue(item.Name, item.Value);
                }
            }

            return DbCommand;
        }

        public virtual void Dispose()
        {
            if (EntityConnection != null)
            {
                EntityConnection.Dispose();
            }

            if (DbCommand != null)
            {
                DbCommand.Dispose();
            }
        }

        public abstract Task<IDataReader> ExecuteDataReaderAsync(string commandText, params DbParameter[] parameters);

        public abstract Task ExecuteNonQueryAsync(string commandText, params DbParameter[] parameters);

        public abstract Task<object> ExecuteScalarAsync(string commandText, params DbParameter[] parameters);
    }
}