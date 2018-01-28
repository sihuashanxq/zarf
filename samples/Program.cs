using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zarf.Generators;
using Zarf.Generators.Functions;
using Zarf.Generators.Functions.Registrars;
using Zarf.Metadata;
using Zarf.Metadata.DataAnnotations;
using Zarf.Query;
using Zarf.Sqlite;
using Zarf.SqlServer;

namespace Zarf
{
    public class WW
    {
        public int Id { get; set; }

        public List<User> C { get; set; }

        public User D { get; set; }
    }

    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext() : base(
            builder => builder.UseSqlServer(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True"))
        {
            Users = Query<User>();
        }

        public IQuery<User> Users { get; }
    }

    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext() : base(
            builder => builder.UseSqlite(@"Data Source=E:\src\zarf\db.db;Version=3"))
        {
            Users = Query<User>();
        }

        public IQuery<User> Users { get; }
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
                var xbn = db.ServiceProvider.GetService(typeof(ISQLFunctionHandlerRegistrar)) as ISQLFunctionHandlerRegistrar;

                var yy = db.Users.Where(n => n.Id.AddTowInt(2) < 10).ToList();
                //SELECT Take Where Skip First FirstOrDefault Single SingleOrDefault Sum Count Avg
                //Order
            }

            using (var db = new SqliteDbContext())
            {
                var xx = db.Users.ToList();
                Console.ReadKey();
            }
        }

        static void BasicTest(DbContext db)
        {
            Console.WriteLine("All..........................");
            db.Query<User>().ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("First..........................");
            Console.WriteLine(db.Query<User>().First());

            Console.WriteLine();
            Console.WriteLine("First id=2.......................");
            Console.WriteLine(db.Query<User>().First(item => item.Id == 2));

            Console.WriteLine();
            Console.WriteLine("Skip 2..........................");
            db.Query<User>().Skip(2).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("Take 2..........................");
            db.Query<User>().Take(2).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("Count..........................");
            Console.WriteLine(db.Query<User>().Count());

            Console.WriteLine();
            Console.WriteLine("Sum..........................");
            Console.WriteLine(db.Query<User>().Sum(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Sum..........................");
            Console.WriteLine(db.Query<User>().Sum(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Where Id>1..........................");
            db.Query<User>().Where(item => item.Id > 1).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("Order By DESC..........................");
            db.Query<User>().OrderByDescending(item => item.Id)
                .ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("CONCAT..........................");
            db.Query<User>().Concat(db.Query<User>()).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("UNION..........................");
            db.Query<User>().Union(db.Query<User>()).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("All Id>0..........................");
            Console.WriteLine(db.Query<User>().All(item => item.Id > 0));

            Console.WriteLine();
            Console.WriteLine("All Id>10000..........................");
            Console.WriteLine(db.Query<User>().All(item => item.Id > 10000));

            Console.WriteLine();
            Console.WriteLine("Id MAX..........................");
            Console.WriteLine(db.Query<User>().Max(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Any Id>0..........................");
            Console.WriteLine(db.Query<User>().Any(item => item.Id > 0));
        }
    }

    public class User
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }

        public int AddressId { get; set; }

        public DateTime BDay { get; set; }

        public IEnumerable<Address> Address { get; set; }

        public override string ToString()
        {
            return "Id:" + Id + "\tAge=" + Age + "\tName=" + Name + "\t AddressId=" + AddressId + "\t BDay=" + BDay.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class Address
    {
        public int Id { get; set; }

        public string Street { get; set; }

        public int UserId { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }

    public class Order
    {
        public int? AddressID { get; set; }

        public string OrderName { get; set; }
    }

    public class PP
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}