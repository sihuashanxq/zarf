using ConsoleApp.Entities;
using System;
using Zarf;
using Zarf.Metadata.Entities;

namespace ConsoleApp
{
    public class Complex
    {
        /// <summary>
        /// Join核心实现
        /// InnerJoin LeftJoin CrossJoin RightJoin FullJoin 扩展方法
        /// </summary>
        /// <param name="db"></param>
        public static void Join(DbContext db)
        {
            var us = db.Query<User>();
            var os = db.Query<Order>();

            var innerJoins = us
                .Join(os, (u, o) => u.Id == o.UserId, JoinType.Inner)
                .Select((u, o) => new { u.Id, o.Goods });

            var leftJoins = us
                .LeftJoin(os, (u, o) => u.Id == o.UserId)
                .Select((u, o) => new { u.Id, o.Goods });

            Console.WriteLine(" User Inner Join Order Id=Id");

            foreach (var item in innerJoins)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine(" User Left Join Order Id = Id");

            foreach (var item in leftJoins)
            {
                Console.WriteLine(item);
            }
        }

        /// <summary>
        /// 子查询
        /// 创建元素的委托难以缓存
        /// </summary>
        /// <param name="db"></param>
        public static void SubQuery(DbContext db)
        {
            //子查询中不能返回IQuery接口类型
            //必须在每一个查询的结尾调用ToList/AsEnumerable
            //(AsEnumerable可以将子查询延迟到第一条记录访问时查询),
            //First,FirstOrDefault,Single/OrDefault,Sum,Aervage,Count,Max,Min之一
            //每一个子查询尽量引用外部查询条件过滤,减少查询数量量
            //如下面的Orders,MaxUserId
            //聚合类的查询会合并到外层查询中,非聚合则内存中过滤
            var uos = db.Query<User>()
                .Where(i => i.Id < 10)
                .Select(i => new
                {
                    UserId = i.Id,
                    Orders = db.Query<Order>().Where(o => o.UserId == i.Id).ToList(),
                    MaxUserId = db.Query<User>().Where(m => m.Id == i.Id).Max(m => m.Id)
                });

            Console.WriteLine(" Sub Query ");

            foreach (var item in uos)
            {
                Console.WriteLine(item);
            }
        }
    }
}
