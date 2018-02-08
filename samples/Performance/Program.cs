using Entities;
using System;
using System.Diagnostics;
using Zarf;
using Zarf.SqlServer;

namespace Performance
{
    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext()
            : base(builder => builder.UseSqlServer(
                @"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True"))
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var st = new Stopwatch();

            using (var db = new SqlServerDbContext())
            {
                //预热
                db.Query<User>().Where(i => i.Id > 0).ToList();

                st.Start();
                for (var j = 0; j < 5; j++)
                {
                    db.Query<User>().Where(m => m.Id > j).ToList();
                }

                st.Stop();
                Console.WriteLine("50万条ToList:" + st.ElapsedMilliseconds / 5.0);

                //预热
                db.Query<User>().Where(m => m.Id > 0).Take(1).ToList();

                st.Reset();
                st.Start();
                for (var j = 0; j < 5; j++)
                {
                    db.Query<User>().Where(m => m.Id > j).Take(400000).ToList();
                }

                st.Stop();
                Console.WriteLine("前40万条ToList:" + st.ElapsedMilliseconds / 5.0);

                //预热
                db.Query<User>().Where(i => i.Id == 0).ToList();
                st.Reset();
                st.Start();

                for (var j = 0; j < 5; j++)
                {
                    for (var i = 0; i < 20000; i++)
                    {
                        db.Query<User>().Where(m => m.Id == i).ToList();
                    }
                }

                st.Stop();
                Console.WriteLine("循环查询2万次,每次查询一条数据:" + st.ElapsedMilliseconds / 5.0);
                Console.WriteLine("Ok");
                Console.WriteLine();
            }
        }
    }
}
