using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

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
    }
}
