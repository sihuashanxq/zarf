using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;

namespace Zarf
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new DbUserContext())
            {
                //BasicTest(db);

                var first = db.Query<PP>().FirstOrDefault();
                var sencond = db.Query<PP>().Skip(1).FirstOrDefault();

                var newPP = new PP()
                {
                    Name = "3333333"
                };

                db.Update(first);
                db.Add(newPP);
                db.Update(sencond);

                //var user = new User()
                //{
                //    Name = "张三",
                //    Age = 33,
                //    BDay = DateTime.Now,
                //    Id = 999
                //};

                //var user2 = new User
                //{
                //    Name = "王五",
                //    Age = 44,
                //    BDay = DateTime.Now,
                //    Id = 1001
                //};

                //db.AddRange(new[] { user, user2 });
                //db.Add(user);
                //db.Users.Add(user);
                //db.Users.AddRange(new[] { user, user2 });

                //user.Name = "李四";

                //db.Users.Update(user);
                //db.Update(user, item => item.Id);

                //user.Age = 22;
                //db.Update(user);

                //db.Delete(user);
                //db.Users.Delete(user);

                //db.Delete(user, item => user.Age);

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
            db.Query<User>().Include(item => item.Address, (x, y) => x.Id == y.UserId).Join(
                db.Query<Address>(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street, user.Address })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("LEFT Join..........................");
            db.Query<User>().Join(
                db.Query<Address>().DefaultIfEmpty(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("Order By DESC..........................");
            db.Query<User>().OrderByDescending(item => item.Id)
                .ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("RIGHT Join..........................");
            db.Query<User>().DefaultIfEmpty().Join(
                db.Query<Address>(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("Full Join..........................");
            db.Query<User>().DefaultIfEmpty()
                .Join(
                db.Query<Address>().DefaultIfEmpty(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

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
                .Include(item => item.Address, (usr, address) => usr.Id == address.UserId && usr.Id != 1)
                //.ThenInclude(item => item.Orders, (address, order) => order.AddressID == address.Id)
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

        public IDbQuery<User> Users { get; }
    }
}