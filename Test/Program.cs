using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Query;
using Zarf.Query.ExpressionVisitors;

namespace Zarf
{
    public class WW
    {
        public int Id { get; set; }

        public List<User> C { get; set; }

        public User D { get; set; }
    }

    class Program
    {
        static async Task<int> A(DbContext db)
        {
            return await db.SaveAsync();
        }

        static void Main(string[] args)
        {
            using (var db = new DbUserContext())
            {
                //SELECT Take Where Skip First FirstOrDefault Single SingleOrDefault Sum Count Avg
                //Order

                /*
                    Select 中包含的子查询
                    1.引用的外层Parameter不能跨作用域
                    2.外层的Parameter引用只能在最后一个表达式方法中
                    3.外层的Parameter不能出现多个
                */
                var xx = db.Users.ToList();
                var fd = xx.Select(item => new
                {
                    Id = item.Id,

                    N = xx.Where(ww => ww.Id < 100).Sum(nn => nn.Age)
                })
                  .Where(item => item.Id < 100).ToList();

                var y = db.Users.Select(item => new
                {
                    Id = item.Id,

                    N = db.Users.Sum(nn => nn.Age)
                })
                .Where(item => item.Id < 100).ToList();

                //var x = db
                //    .Users
                //    .Select(item => new { item.Id, item.Name })
                //    .Where(item => db.Users.Where(y => item.Id == 5).ToList() != null).ToList();

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
            Console.WriteLine("Inner Join..........................");

            Console.WriteLine();
            Console.WriteLine("Order By DESC..........................");
            db.Query<User>().OrderByDescending(item => item.Id)
                .ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("RIGHT Join..........................");

            Console.WriteLine();
            Console.WriteLine("Full Join..........................");

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
            Console.WriteLine("All Id MAX..........................");
            Console.WriteLine(db.Query<User>().Max(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Any Id>0..........................");
            Console.WriteLine(db.Query<User>().Any(item => item.Id > 0));

            Console.WriteLine("Include Test");
            var users = db.Query<User>()
                .Include(item => item.Address)
                .ThenInclude(item => item.Orders, (address, order) => order.AddressID == address.Id)
                .Select(item => item)
                .ToList();
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

    public class DbUserContext : SqlServerDbContext
    {
        public DbUserContext() :
            base(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True")
        {
            Users = this.Query<User>();
        }

        public IQuery<User> Users { get; }
    }
}