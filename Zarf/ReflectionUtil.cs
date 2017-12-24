using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zarf.Core;

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
            Include = typeof(QueryExtension).GetMethod("Include", BindingFlags.NonPublic | BindingFlags.Static);
            ThenInclude = typeof(QueryExtension).GetMethod("ThenInclude", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static IEnumerable<TEntity> Where<TOEntity, TEntity>(this IEnumerable<TEntity> entities, TOEntity oEntity, Func<TOEntity, TEntity, bool> predicate)
        {
            foreach (var entity in entities)
            {
                if (predicate(oEntity, entity))
                {
                    yield return entity;
                }
            }
        }

        /// <summary>
        /// Get The  Method Of <see cref="Query{TEntity}"/> 
        /// Mapped <see cref="Queryable"/> Extension Method
        /// </summary>
        /// <param name="givenMethod">The Method Of <see cref="Query{TEntity}"/></param>
        /// <returns><see cref="MethodInfo"/></returns>
        public static MethodInfo FindSameDefinitionQueryableMethod(MethodInfo givenMethod, Type giveType)
        {
            var querableCandidates = QueryableMethods.Where(item => item.Name == givenMethod.Name);
            var givenParameters = givenMethod.GetParameters();

            foreach (var item in querableCandidates.Concat(typeof(Enumerable).GetMethods().Where(item => item.Name == "ToList")))
            {
                var condidation = item.IsGenericMethod ? item.MakeGenericMethod(giveType) : item;
                var condParameters = condidation.GetParameters();
                if (condParameters.Length != givenParameters.Length + 1)
                {
                    continue;
                }

                var matched = true;
                for (var i = 1; i < condParameters.Length; i++)
                {
                    if (condParameters[i].ParameterType != givenParameters[i - 1].ParameterType)
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return condidation;
                }
            }

            throw new Exception($"can not find {givenMethod.Name}the mapped Queryable Method");
        }
    }
}
