using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Zarf.Entities;

namespace Zarf
{
    public class DbCommand
    {
        protected string CommandText { get; }

        public DbCommand(string commandText)
        {
            CommandText = commandText;
        }

        public IDataReader ExecuteReader()
        {
            var dbConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True");
            var dbCommand = new SqlCommand(CommandText, dbConnection);
            dbConnection.Open();
            return dbCommand.ExecuteReader();
        }

        public object ExecuteScalar(string commandText, params DbParameter[] dbParams)
        {
            var dbConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True");
            var dbCommand = new SqlCommand(commandText, dbConnection);
            foreach (var dbParam in dbParams)
            {
                dbCommand.Parameters.AddWithValue(dbParam.Name, dbParam.Value);
            }

            dbConnection.Open();
            return dbCommand.ExecuteScalar();
        }

        public void ExecuteNonQuery(string commandText, params DbParameter[] dbParams)
        {
            var dbConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True");
            var dbCommand = new SqlCommand(commandText, dbConnection);
            foreach (var dbParam in dbParams)
            {
                dbCommand.Parameters.AddWithValue(dbParam.Name, dbParam.Value);
            }

            dbConnection.Open();
            dbCommand.ExecuteNonQuery();
        }
    }
}
