using System;
using System.Data;
using System.Reflection;

namespace Zarf.Mapping.Bindings.Binders
{
    public class MemberValueGetter
    {
        public MethodInfo GetValueGetterMethod(Type memberType)
        {
            if (memberType == ReflectionUtil.CharType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetChar));
            }

            if (memberType == ReflectionUtil.CharNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetCharNullable));
            }

            if (memberType == ReflectionUtil.ByteType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByte));
            }

            if (memberType == ReflectionUtil.ByteNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.StringType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetString));
            }

            if (memberType == ReflectionUtil.IntType ||
                memberType == ReflectionUtil.UIntType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetInt32));
            }

            if (memberType == ReflectionUtil.IntNullableType ||
                memberType == ReflectionUtil.UIntNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetInt32Nullable));
            }

            if (memberType == ReflectionUtil.ShortType ||
                memberType == ReflectionUtil.UShortType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetInt16));
            }

            if (memberType == ReflectionUtil.ShortNullableType ||
                memberType == ReflectionUtil.UShortNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetInt16Nullable));
            }

            if (memberType == ReflectionUtil.LongType ||
                memberType == ReflectionUtil.ULongType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetInt64));
            }

            if (memberType == ReflectionUtil.LongNullbaleType ||
                memberType == ReflectionUtil.ULongNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetInt64Nullable));
            }

            if (memberType == ReflectionUtil.ByteNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.DecimalType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.DecimalNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.FloatType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.FloatNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.DoubleType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.DoubleNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.DateTimeType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.DateTimeNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.GuidType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.GuidNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.BooleanType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            if (memberType == ReflectionUtil.BooleanNullableType)
            {
                return typeof(MemberValueGetter).GetMethod(nameof(GetByteNullable));
            }

            throw new NotImplementedException("ValueGetter !!!");

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
