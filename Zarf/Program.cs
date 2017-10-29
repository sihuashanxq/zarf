using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Zarf.Mapping;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf
{

    public class User
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }

        public int AddressId { get; set; }

        public DateTime BDay { get; set; }

        public IEnumerable<Address> Address { get; set; }

        public IEnumerable<Order> Orders { get; set; }

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

    public class Abc
    {
        public int Id { get; set; }

        public int Count;
    }


    public class C
    {
        public User User { get; set; }

        public int Id { get; set; }
    }

    public class PP
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var db = new DbContext();

            var y = db.DbQuery<User>()
                .Include(item => item.Address, (user, address) => user.Id == address.UserId && user.Id != 1)
                .Select(item => item)
                .ToList();

            //.ThenInclude(item => item.Orders, (address, order) => order.AddressID == address.Id)
            //BasicTest(db);
            var p = new PP
            {
                Name = "333"
            };

            db.Add(p);

            Console.ReadKey();
        }

        public static MethodInfo Method = typeof(Dictionary<,>).MakeGenericType(
            new Type[] { typeof(MemberInfo), typeof(object) }).GetMethod("Add");

        public static Dictionary<MemberInfo, object> GetDictionary(object typeOf)
        {
            var mem = EntityTypeDescriptorFactory.Factory.Create(typeOf.GetType());
            var dic = new Dictionary<MemberInfo, object>();
            var mems = new List<Expression>();
            var dd = Expression.Parameter(typeof(Dictionary<MemberInfo, object>));
            foreach (var item in mem.GetExpandMembers())
            {
                mems.Add(Expression.Call(dd,
                    Method,
                    Expression.Constant(item),
                   Expression.Convert(Expression.MakeMemberAccess(Expression.Constant(typeOf), item), typeof(object))
                    ));
            }

            var begin = Expression.Label();
            var end = Expression.Label(begin);
            mems.Add(end);
            var st = new System.Diagnostics.Stopwatch();
            var d = new Dictionary<MemberInfo, object>();

            st.Start();

            for (var i = 0; i < 10000; i++)
            {
                d = new Dictionary<MemberInfo, object>();
                foreach (var item in mem.GetExpandMembers())
                {
                    if (item is PropertyInfo)
                    {
                        d[item] = (item as PropertyInfo).GetValue(typeOf);
                    }
                    else
                    {
                        d[item] = (item as FieldInfo).GetValue(typeOf);
                    }
                }
            }
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);

            st.Reset();
            var x = (Action<Dictionary<MemberInfo, object>>)Expression.Lambda(Expression.Block(mems), dd).Compile();
            st.Start();
            for (var i = 0; i < 10000; i++)
            {
                dic = new Dictionary<MemberInfo, object>();
                x(dic);
            }
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);
            return dic;
        }

        static void BasicTest(DbContext db)
        {
            Console.WriteLine("All..........................");
            db.DbQuery<User>().ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("First..........................");
            Console.WriteLine(db.DbQuery<User>().First());

            Console.WriteLine();
            Console.WriteLine("First id=2.......................");
            Console.WriteLine(db.DbQuery<User>().First(item => item.Id == 2));

            Console.WriteLine();
            Console.WriteLine("Skip 2..........................");
            db.DbQuery<User>().Skip(2).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("Take 2..........................");
            db.DbQuery<User>().Take(2).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("Count..........................");
            Console.WriteLine(db.DbQuery<User>().Count());

            Console.WriteLine();
            Console.WriteLine("Sum..........................");
            Console.WriteLine(db.DbQuery<User>().Sum(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Sum..........................");
            Console.WriteLine(db.DbQuery<User>().Sum(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Where Id>1..........................");
            db.DbQuery<User>().Where(item => item.Id > 1).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("Inner Join..........................");
            db.DbQuery<User>().Join(
                db.DbQuery<Address>(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("LEFT Join..........................");
            db.DbQuery<User>().Join(
                db.DbQuery<Address>().DefaultIfEmpty(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("Order By DESC..........................");
            db.DbQuery<User>().OrderByDescending(item => item.Id)
                .ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("RIGHT Join..........................");
            db.DbQuery<User>().DefaultIfEmpty().Join(
                db.DbQuery<Address>(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("Full Join..........................");
            db.DbQuery<User>().DefaultIfEmpty().Join(
                db.DbQuery<Address>().DefaultIfEmpty(),
                item => item.AddressId,
                item => item.Id,
                (user, address) => new { user.Name, address.Street })
                .ToList().ForEach(item => Console.WriteLine($"Name:{item.Name} Street:{item.Street}"));

            Console.WriteLine();
            Console.WriteLine("CONCAT..........................");
            db.DbQuery<User>().Concat(db.DbQuery<User>()).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("UNION..........................");
            db.DbQuery<User>().Union(db.DbQuery<User>()).ToList().ForEach(item => Console.WriteLine(item));

            Console.WriteLine();
            Console.WriteLine("All Id>0..........................");
            Console.WriteLine(db.DbQuery<User>().All(item => item.Id > 0));

            Console.WriteLine();
            Console.WriteLine("All Id>10000..........................");
            Console.WriteLine(db.DbQuery<User>().All(item => item.Id > 10000));

            Console.WriteLine();
            Console.WriteLine("All Id MAX..........................");
            Console.WriteLine(db.DbQuery<User>().Max(item => item.Id));

            Console.WriteLine();
            Console.WriteLine("Any Id>0..........................");
            Console.WriteLine(db.DbQuery<User>().Any(item => item.Id > 0));
        }
    }

}