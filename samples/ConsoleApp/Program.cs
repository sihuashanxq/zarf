using Entities;
using Zarf;
using Zarf.Sqlite;
using Zarf.SqlServer;

namespace ConsoleApp
{
    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext() : base(
            builder => builder.UseSqlServer(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True"))
        {
        }

        public IQuery<User> Users => Query<User>();
    }

    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext() : base(
            builder => builder.UseSqlite(@"Data Source=E:\src\zarf\db.db;Version=3"))
        {

        }

        public IQuery<User> Users => Query<User>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new SqliteDbContext())
            {
                //QueryDemo.Query(db);
                ModifyDemo.Insert(db);
                ModifyDemo.Update(db);
                ModifyDemo.Delete(db);
                //ModifyDemo.AsyncModify(db);
            }
        }
    }
}