using Entities;
using System;
using Zarf;

namespace ConsoleApp
{
    public static class ModifyDemo
    {
        public static void Insert(DbContext db)
        {
            var u = new User() { Id = 10000000, Name = "abc", CreateDay = DateTime.Now, Age = 18, Tel = "123" };

            db.Add(u);
            db.AddRange(new[] { u });

            db.Save();
        }

        public static void Update(DbContext db)
        {
            var u = db.Query<User>().FirstOrDefault();

            db.TrackEntity(u);

            u.Name = "张三";

            db.Update(u);

            db.Save();
        }

        public static void Delete(DbContext db)
        {
            db.Delete(new User() { Id = -1 });
            db.Save();
        }

        public static void Transaction(DbContext db)
        {
            var trans = db.BeginTransaction();

            try
            {
                var u = db.Query<User>().FirstOrDefault();

                db.TrackEntity(u);

                u.Name = "张三";

                db.Update(u);
                db.Delete(new User() { Id = 1 });

                db.Save();
                trans.Commit();
            }

            catch (Exception)
            {
                trans.Rollback();
            }
        }

        public static async void AsyncModify(DbContext db)
        {
            using (var trans = await db.BeginTransactionAsync())
            {
                try
                {
                    var u = db.Query<User>().FirstOrDefault();

                    db.TrackEntity(u);

                    u.Name = "张三";

                    await db.UpdateAsync(u);
                    await db.DeleteAsync(new User() { Id = 1 });
                    await db.SaveAsync();

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }
        }
    }
}
