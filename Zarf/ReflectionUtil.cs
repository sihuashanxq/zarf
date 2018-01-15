using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Extensions;

namespace Zarf
{
    public static class ReflectionUtil
    {
        public static MethodInfo SubQueryWhere { get; }

        public static MethodInfo Include { get; }

        public static MethodInfo ThenInclude { get; }

        public static MethodInfo Join { get; }

        public static MethodInfo JoinSelect { get; }

        public static MethodInfo[] QueryableMethods { get; }

        public static readonly Type CharType = typeof(char);

        public static readonly Type CharNullableType = typeof(char?);

        public static readonly Type ByteType = typeof(byte);

        public static readonly Type ByteNullableType = typeof(byte?);

        public static readonly Type StringType = typeof(string);

        public static readonly Type IntType = typeof(int);

        public static readonly Type IntNullableType = typeof(int?);

        public static readonly Type UIntType = typeof(uint);

        public static readonly Type UIntNullableType = typeof(uint?);

        public static readonly Type ShortType = typeof(short);

        public static readonly Type ShortNullableType = typeof(short?);

        public static readonly Type UShortType = typeof(ushort);

        public static readonly Type UShortNullableType = typeof(ushort?);

        public static readonly Type LongType = typeof(long);

        public static readonly Type LongNullbaleType = typeof(long?);

        public static readonly Type ULongType = typeof(ulong);

        public static readonly Type ULongNullableType = typeof(ulong?);

        public static readonly Type DecimalType = typeof(decimal);

        public static readonly Type DecimalNullableType = typeof(decimal?);

        public static readonly Type FloatType = typeof(float);

        public static readonly Type FloatNullableType = typeof(float?);

        public static readonly Type DoubleType = typeof(double);

        public static readonly Type DoubleNullableType = typeof(double?);

        public static readonly Type DateTimeType = typeof(DateTime);

        public static readonly Type DateTimeNullableType = typeof(DateTime?);

        public static readonly Type GuidType = typeof(Guid);

        public static readonly Type GuidNullableType = typeof(Guid?);

        public static readonly Type BooleanType = typeof(bool);

        public static readonly Type BooleanNullableType = typeof(bool?);

        /// <summary>
        /// 简单类型
        /// </summary>
        public static HashSet<Type> SimpleTypes = new HashSet<Type>()
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
            QueryableMethods = typeof(Queryable).GetMethods();
            SubQueryWhere = typeof(ReflectionUtil).GetMethod(nameof(Where));
            Join = typeof(JoinQuery).GetMethod("Join", BindingFlags.NonPublic | BindingFlags.Static);
            JoinSelect = typeof(JoinQuery).GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static IEnumerable<TEntity> Where<TEntity>(this IEnumerable<TEntity> entities, Func<TEntity, bool> predicate)
        {
            return Enumerable.Where(entities, predicate);
        }

        /// <summary>
        /// Get The  Method Of <see cref="Query{TEntity}"/> 
        /// Mapped <see cref="Queryable"/> Extension Method
        /// </summary>
        /// <param name="method">The Method Of <see cref="Query{TEntity}"/></param>
        /// <returns><see cref="MethodInfo"/></returns>
        public static MethodInfo FindQueryableMethod(MethodInfo method, Type typeOfEntity, Type typeOfResult)
        {
            Func<MethodInfo, MethodInfo> makeGenericMethod = (m) =>
            {
                if (!m.IsGenericMethod)
                {
                    return m;
                }

                if (m.GetGenericArguments().Length == 2)
                {
                    return m.MakeGenericMethod(typeOfEntity, typeOfResult);
                }

                return m.MakeGenericMethod(typeOfEntity);
            };

            var parameters = method.GetParameters();
            var conds = typeof(IQuery).IsAssignableFrom(method.DeclaringType)
                ? ZarfQueryable.Methods.Where(item => item.Name == method.Name).ToList()
                : ReflectionUtil.QueryableMethods.Where(item => item.Name == method.Name).ToList();

            foreach (var cond in conds)
            {
                var genericCondMethod = makeGenericMethod(cond);
                var genericCondParameters = genericCondMethod.GetParameters();
                if (genericCondParameters.Length != parameters.Length + 1)
                {
                    continue;
                }

                var i = 1;
                while (i < genericCondParameters.Length)
                {
                    if (genericCondParameters[i].ParameterType != parameters[i - 1].ParameterType)
                    {
                        break;
                    }
                }

                if (i >= genericCondParameters.Length)
                {
                    return genericCondMethod;
                }
            }

            throw new Exception($"can not find {method.Name}the mapped Queryable Method");
        }
    }

    public static class QueryEnumerable
    {
        public static MethodInfo ToListMethod = typeof(QueryEnumerable).GetMethod(nameof(ToList));

        public static MethodInfo FirstOrDefaultMethod = typeof(QueryEnumerable).GetMethod(nameof(FirstOrDefault));

        public static MethodInfo FirstOrDefaultHasParameterMethod = typeof(QueryEnumerable).GetMethod(nameof(FirstOrDefault2));

        public static List<TSource> ToList<TSource>(IEnumerable<TSource> source)
        {
            return source.ToList();
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source)
        {
            return source.FirstOrDefault();
        }

        public static TSource FirstOrDefault2<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.FirstOrDefault(predicate);
        }
    }
}
