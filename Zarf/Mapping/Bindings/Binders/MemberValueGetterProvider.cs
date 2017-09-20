using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;

namespace Zarf.Mapping.Bindings.Binders
{
    public class MemberValueGetterProvider
    {
        private static Dictionary<Type, MethodInfo> ValueGetterCache;

        public static readonly MemberValueGetterProvider DefaultProvider = new MemberValueGetterProvider();

        private MemberValueGetterProvider()
        {

        }

        static MemberValueGetterProvider()
        {
            ValueGetterCache = new Dictionary<Type, MethodInfo>
            {
                [ReflectionUtil.StringType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetString)),
                [ReflectionUtil.CharType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetChar)),
                [ReflectionUtil.ByteType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetByte)),
                [ReflectionUtil.IntType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt32)),
                [ReflectionUtil.UIntType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt32)),
                [ReflectionUtil.ShortType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt16)),
                [ReflectionUtil.UShortType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt16)),
                [ReflectionUtil.LongType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt64)),
                [ReflectionUtil.ULongType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt64)),
                [ReflectionUtil.DecimalType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetDecimal)),
                [ReflectionUtil.FloatType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetFloat)),
                [ReflectionUtil.DoubleType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetDouble)),
                [ReflectionUtil.DateTimeType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetDateTime)),
                [ReflectionUtil.GuidType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetGuid)),
                [ReflectionUtil.BooleanType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetBoolean)),

                [ReflectionUtil.CharNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetCharNullable)),
                [ReflectionUtil.ByteNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetByteNullable)),
                [ReflectionUtil.IntNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt32Nullable)),
                [ReflectionUtil.UIntNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt32Nullable)),
                [ReflectionUtil.ShortNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt16Nullable)),
                [ReflectionUtil.UShortNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt16Nullable)),
                [ReflectionUtil.LongNullbaleType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt64Nullable)),
                [ReflectionUtil.ULongNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetInt64Nullable)),
                [ReflectionUtil.DecimalNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetDecimalNullable)),
                [ReflectionUtil.FloatNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetFloatNullable)),
                [ReflectionUtil.DoubleNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetDoubleNullable)),
                [ReflectionUtil.DateTimeNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetDateTimeNullable)),
                [ReflectionUtil.GuidNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetGuidNullable)),
                [ReflectionUtil.BooleanNullableType] = typeof(MemberValueGetterProvider).GetMethod(nameof(GetBooleanNullable))
            };
        }

        public MethodInfo GetValueGetter(Type memberType)
        {
            if (!ValueGetterCache.ContainsKey(memberType))
            {
                throw new NotImplementedException("ValueGetter !!!");
            }

            return ValueGetterCache[memberType];
        }

        internal static bool GetBoolean(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return false;
            }

            return dataReader.GetBoolean(ordianl);
        }

        internal static byte GetByte(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return byte.MinValue;
            }

            return dataReader.GetByte(ordianl);
        }

        internal static char GetChar(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return char.MinValue;
            }

            return dataReader.GetChar(ordianl);
        }

        internal static DateTime GetDateTime(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return DateTime.MinValue;
            }

            return dataReader.GetDateTime(ordianl);
        }

        internal static decimal GetDecimal(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return 0;
            }

            return dataReader.GetDecimal(ordianl);
        }

        internal static double GetDouble(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return 0;
            }

            return dataReader.GetDouble(ordianl);
        }

        internal static float GetFloat(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return 0;
            }

            return dataReader.GetFloat(ordianl);
        }

        internal static Guid GetGuid(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return Guid.Empty;
            }

            return dataReader.GetGuid(ordianl);
        }

        internal static short GetInt16(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return 0;
            }

            return dataReader.GetInt16(ordianl);
        }

        internal static int GetInt32(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return 0;
            }

            return dataReader.GetInt32(ordianl);
        }

        internal static long GetInt64(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return 0;
            }

            return dataReader.GetInt64(ordianl);
        }

        internal static bool? GetBooleanNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetBoolean(ordianl);
        }

        internal static byte? GetByteNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetByte(ordianl);
        }

        internal static char? GetCharNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetChar(ordianl);
        }

        internal static DateTime? GetDateTimeNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetDateTime(ordianl);
        }

        internal static decimal? GetDecimalNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetDecimal(ordianl);
        }

        internal static double? GetDoubleNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetDouble(ordianl);
        }

        internal static float? GetFloatNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetFloat(ordianl);
        }

        internal static Guid? GetGuidNullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetGuid(ordianl);
        }

        internal static short? GetInt16Nullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetInt16(ordianl);
        }

        internal static int? GetInt32Nullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetInt32(ordianl);
        }

        internal static long? GetInt64Nullable(IDataReader dataReader, int ordianl)
        {
            if (dataReader.IsDBNull(ordianl))
            {
                return null;
            }

            return dataReader.GetInt64(ordianl);
        }

        internal static string GetString(IDataReader dataReader, int ordianl)
        {
            return dataReader.GetString(ordianl);
        }
    }
}
