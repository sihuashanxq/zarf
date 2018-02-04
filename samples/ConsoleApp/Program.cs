using ConsoleApp;
using ConsoleApp.Entities;
using ConsoleApp.Queries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zarf;
using Zarf.Generators;
using Zarf.Generators.Functions;
using Zarf.Generators.Functions.Registrars;
using Zarf.Metadata;
using Zarf.Metadata.DataAnnotations;
using Zarf.Query;
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

    public static class IntExtension
    {
        [SQLFunctionHandler(typeof(IntFunHandler))]
        public static int AddTowInt(this int i, int n)
        {
            return i + n;
        }
    }

    public class IntFunHandler : ISQLFunctionHandler
    {
        public Type SoupportedType => typeof(int);

        public bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == "AddTowInt")
            {
                generator.Attach(methodCall.Arguments[0]);
                generator.Attach(" + ");
                generator.Attach(methodCall.Arguments[1]);
                return true;
            }

            return false;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new SqlServerDbContext())
            {
                var st = new Stopwatch();
                st.Start();

                //SimpleQuery.SimpleQuery(db);
                Function.Query(db);
                Complex.SubQuery(db);
                Complex.Join(db);

                st.Stop();
                Console.WriteLine(st.ElapsedMilliseconds);
            }
        }
    }

    public class Address
    {
        public int Id { get; set; }

        public string Street { get; set; }

        public int UserId { get; set; }
    }
}