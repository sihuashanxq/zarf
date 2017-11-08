using System.Data;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IDataBaseFacade
    {
        IDbCommandFacade GetCommand();
    }

    public interface IDbCommandFacotry
    {
        IDbCommandWrapper Create(IDbConnectionWrapper dbConnection);

        IDbCommandWrapper Create();

        IDbCommandWrapper Create(IDbConnection dbConnection);
    }

    public interface IDbConnectionFacotry
    {
        IDbConnectionWrapper Create();
    }

    public interface IDbConnectionWrapper
    {
        IDbConnection DbConnection { get; }
        string ConnectionString { get; }
    }

    public interface IDbCommandWrapper
    {
        IDbConnectionWrapper DbConnection { get; }

        IDbCommand DbCommand { get; }

        void BeginTransaction();

        void RollbackTransaction();

        void CommitTransaction();

        IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams);

        void ExecuteNonQuery(string commandText, params DbParameter[] dbParams);

        TValueType ExecuteScalar<TValueType>(string commandText, params DbParameter[] dbParams);

        void AddParameterWithValue(string parameterName, object value);
    }

    public class DbCommandWrapper : IDbCommandWrapper
    {
        public IDbConnectionWrapper DbConnection { get; }

        public IDbCommand DbCommand { get; }

        public DbCommandWrapper(IDbCommand dbCommand)
        {
            DbCommand = dbCommand;
        }

        public IDataReader ExecuteDataReader(string commandText, params DbParameter[] dbParams)
        {
            return null;
        }

        public TValueType ExecuteScalar<TValueType>(string commandText, params DbParameter[] dbParams)
        {
            return default(TValueType);
        }

        public void ExecuteNonQuery(string commandText, params DbParameter[] dbParams)
        {

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

        }
    }

    public class DbConnectionWrapper : IDbConnectionWrapper
    {
        public string ConnectionString => DbConnection.ConnectionString;

        public IDbConnection DbConnection { get; }

        public DbConnectionWrapper(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }
    }
}
