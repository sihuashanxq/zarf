using System;
using System.Linq.Expressions;
using Zarf.Generators;
using Zarf.Generators.Functions;
using Zarf.Metadata.DataAnnotations;

namespace ConsoleApp
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
}
