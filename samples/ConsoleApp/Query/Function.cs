using ConsoleApp.Entities;
using System;
using System.Linq.Expressions;
using Zarf;
using Zarf.Generators;
using Zarf.Generators.Functions;
using Zarf.Metadata.DataAnnotations;

namespace ConsoleApp.Queries
{
    /// <summary>
    /// int 扩展
    /// </summary>
    public static class IntExtension
    {
        [SQLFunctionHandler(typeof(IntSQLFunctionHandler))]
        public static int Add(this int i, int n)
        {
            return i + n;
        }
    }

    /// <summary>
    /// 处理Function的SQL生成
    /// </summary>
    public class IntSQLFunctionHandler : ISQLFunctionHandler
    {
        public Type SoupportedType => typeof(int);

        public bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            //SQL 生成
            if (methodCall.Method.Name == "Add")
            {
                generator.Attach(methodCall.Arguments[0]);
                generator.Attach(" + ");
                generator.Attach(methodCall.Arguments[1]);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 函数处理
    /// </summary>
    public class Function
    {
        public static void Query(DbContext db)
        {
            var idAdd2 = db.Query<User>().Where(i => i.Id.Add(2) < 10)
                .Select(i => new
                {
                    IdAdd3 = i.Id.Add(3),
                    Name = i.Name
                });

            Console.WriteLine("Function ");

            foreach (var item in idAdd2)
            {
                Console.WriteLine(item);
            }
        }
    }
}
