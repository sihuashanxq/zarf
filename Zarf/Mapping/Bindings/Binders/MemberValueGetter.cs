using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;

namespace Zarf.Mapping.Bindings.Binders
{
    public class MemberValueGetter
    {
        public static Dictionary<Type, MethodInfo> ValueGetterCache;

        static MemberValueGetter()
        {
            ValueGetterCache = new Dictionary<Type, MethodInfo>();

            ValueGetterCache[ReflectionUtil.StringType] = typeof(MemberValueGetter).GetMethod(nameof(GetString));
            ValueGetterCache[ReflectionUtil.CharType] = typeof(MemberValueGetter).GetMethod(nameof(GetChar));
            ValueGetterCache[ReflectionUtil.ByteType] = typeof(MemberValueGetter).GetMethod(nameof(GetByte));
            ValueGetterCache[ReflectionUtil.IntType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt32));
            ValueGetterCache[ReflectionUtil.UIntType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt32));
            ValueGetterCache[ReflectionUtil.ShortType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt16));
            ValueGetterCache[ReflectionUtil.UShortType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt16));
            ValueGetterCache[ReflectionUtil.LongType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt64));
            ValueGetterCache[ReflectionUtil.ULongType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt64));
            ValueGetterCache[ReflectionUtil.DecimalType] = typeof(MemberValueGetter).GetMethod(nameof(GetDecimal));
            ValueGetterCache[ReflectionUtil.FloatType] = typeof(MemberValueGetter).GetMethod(nameof(GetFloat));
            ValueGetterCache[ReflectionUtil.DoubleType] = typeof(MemberValueGetter).GetMethod(nameof(GetDouble));
            ValueGetterCache[ReflectionUtil.DateTimeType] = typeof(MemberValueGetter).GetMethod(nameof(GetDateTime));
            ValueGetterCache[ReflectionUtil.GuidType] = typeof(MemberValueGetter).GetMethod(nameof(GetGuid));
            ValueGetterCache[ReflectionUtil.BooleanType] = typeof(MemberValueGetter).GetMethod(nameof(GetBoolean));

            ValueGetterCache[ReflectionUtil.CharNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetCharNullable));
            ValueGetterCache[ReflectionUtil.ByteNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            ValueGetterCache[ReflectionUtil.IntNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt32Nullable));
            ValueGetterCache[ReflectionUtil.UIntNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt32Nullable));
            ValueGetterCache[ReflectionUtil.ShortNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt16Nullable));
            ValueGetterCache[ReflectionUtil.UShortNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt16Nullable));
            ValueGetterCache[ReflectionUtil.LongNullbaleType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt64Nullable));
            ValueGetterCache[ReflectionUtil.ULongNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetInt64Nullable));
            ValueGetterCache[ReflectionUtil.DecimalNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetDecimalNullable));
            ValueGetterCache[ReflectionUtil.FloatNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetFloatNullable));
            ValueGetterCache[ReflectionUtil.DoubleNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetDoubleNullable));
            ValueGetterCache[ReflectionUtil.DateTimeNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetDateTimeNullable));
            ValueGetterCache[ReflectionUtil.GuidNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetGuidNullable));
            ValueGetterCache[ReflectionUtil.BooleanNullableType] = typeof(MemberValueGetter).GetMethod(nameof(GetBooleanNullable));
        }

        public MethodInfo GetValueGetterMethod(Type memberType)
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
