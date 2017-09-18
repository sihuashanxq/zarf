using System.Reflection;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

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

        public static MethodInfo EnumerableDistinct { get; }

        public static MethodInfo[] AllQueryableMethods { get; }

        public static MethodInfo[] AllEnumerableMethods { get; }

        public static Type CharType = typeof(char);

        public static Type CharNullableType = typeof(char?);

        public static Type ByteType = typeof(byte);

        public static Type ByteNullableType = typeof(byte?);

        public static Type StringType = typeof(string);

        public static Type IntType = typeof(int);

        public static Type IntNullableType = typeof(int?);

        public static Type UIntType = typeof(uint);

        public static Type UIntNullableType = typeof(uint?);

        public static Type ShortType = typeof(short);

        public static Type ShortNullableType = typeof(short?);

        public static Type UShortType = typeof(ushort);

        public static Type UShortNullableType = typeof(ushort?);

        public static Type LongType = typeof(long);

        public static Type LongNullbaleType = typeof(long?);

        public static Type ULongType = typeof(ulong);

        public static Type ULongNullableType = typeof(ulong?);

        public static Type DecimalType = typeof(decimal);

        public static Type DecimalNullableType = typeof(decimal?);

        public static Type FloatType = typeof(float);

        public static Type FloatNullableType = typeof(float?);

        public static Type DoubleType = typeof(double);

        public static Type DoubleNullableType = typeof(double?);

        public static Type DateTimeType = typeof(DateTime);

        public static Type DateTimeNullableType = typeof(DateTime?);

        public static Type GuidType = typeof(DateTime);

        public static Type GuidNullableType = typeof(DateTime?);

        public static Type BooleanType = typeof(DateTime);

        public static Type BooleanNullableType = typeof(DateTime?);

        /// <summary>
        /// 简单类型
        /// </summary>
        public static Type[] SimpleTypes = new[]
        {
            CharType,
            CharNullableType,
            ByteType,
            ByteNullableType,
            StringType,
            IntType,
            IntNullableType,
            UIntType,
            UIntNullableType,
            ShortType,
            ShortNullableType,
            UShortType,
            UShortNullableType,
            LongType,
            LongNullbaleType,
            ULongType,
            ULongNullableType,
            DecimalType,
            DecimalNullableType,
            FloatType,
            FloatNullableType,
            DoubleType,
            DoubleNullableType,
            DateTimeType,
            DateTimeNullableType,
            GuidType,
            GuidNullableType,
            BooleanType,
            BooleanNullableType
        };

        static ReflectionUtil()
        {
            AllQueryableMethods = typeof(Queryable).GetMethods();
            AllEnumerableMethods = typeof(Enumerable).GetMethods();

            EnumerableWhereMethod = typeof(ReflectionUtil).GetMethod(nameof(Where));
            EnumerableDistinct = AllEnumerableMethods.FirstOrDefault(item => item.Name == "Distinct");

            //.FirstOrDefault(item => item.Name == "Where" && item.GetParameters().Last().ParameterType.GenericTypeArguments.Length == 2);
            EnumerableToListMethod = AllEnumerableMethods.FirstOrDefault(item => item.Name == "ToList");
        }

        public static IEnumerable<V> Where<T, V>(this IEnumerable<V> enumerable, T t, Func<T, V, bool> func)
        {
            foreach (var item in enumerable)
            {
                if (func(t, item))
                {
                    yield return item;
                }
            }
        }
    }
}
