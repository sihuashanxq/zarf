using System.Reflection;
using System.Linq;
using System;

namespace Zarf
{
    /// <summary>
    /// 反射工具
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// Enumerable.Where
        /// </summary>
        public static MethodInfo EnumerableWhereMethod { get; }

        /// <summary>
        /// Enumerable.ToList
        /// </summary>
        public static MethodInfo EnumerableToListMethod { get; }

        public static MethodInfo[] AllQueryableMethods { get; }

        public static MethodInfo[] AllEnumerableMethods { get; }

        /// <summary>
        /// 简单类型
        /// </summary>
        public static Type[] SimpleTypes = new[]
        {
            typeof(char),
            typeof(char?),
            typeof(byte),
            typeof(byte?),
            typeof(string),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(decimal),
            typeof(decimal?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(Guid),
            typeof(Guid?),
            typeof(bool),
            typeof(bool?)
        };

        static ReflectionUtil()
        {
            AllQueryableMethods = typeof(Queryable).GetMethods();
            AllEnumerableMethods = typeof(Enumerable).GetMethods();

            EnumerableWhereMethod = AllEnumerableMethods.FirstOrDefault(item => item.Name == "Where" && item.GetParameters().Last().ParameterType.GenericTypeArguments.Length == 2);
            EnumerableToListMethod = AllEnumerableMethods.FirstOrDefault(item => item.Name == "ToList");
        }
    }
}
