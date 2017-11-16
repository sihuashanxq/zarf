using System;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using System.Linq;
namespace Zarf.Update
{
    public class MemberDescriptor
    {
        static Type[] NumericTypes = new[]
        {
            typeof(byte),typeof(byte?),
            typeof(int),typeof(int?),
            typeof(uint),typeof(uint?),
            typeof(short),typeof(short?),
            typeof(ushort),typeof(ushort?),
            typeof(long),typeof(long?),
            typeof(ulong),typeof(ulong?),
            typeof(decimal),typeof(decimal?),
            typeof(float),typeof(float?),
        };

        public bool IsPrimary { get; }

        public bool IsAutoIncrement { get; }

        public bool IgnoreMapped { get; }

        public MemberInfo Member { get; }

        public bool IsWriteable
            => Member.Is<FieldInfo>() || (Member.As<PropertyInfo>()?.CanWrite ?? false);

        public bool IsReadable
            => Member.Is<FieldInfo>() || (Member.As<PropertyInfo>()?.CanRead ?? false);

        public object GetValue(object obj)
        {
            return Member.MemberType == MemberTypes.Field
                ? (Member as FieldInfo)?.GetValue(obj)
                : (Member as PropertyInfo)?.GetValue(obj);
        }

        public void SetValue(object entity, object value)
        {
            if (Member.MemberType == MemberTypes.Field)
            {
                (Member as FieldInfo)?.SetValue(entity, value);
            }
            else
            {
                (Member as PropertyInfo)?.SetValue(entity, value);
            }
        }

        public MemberDescriptor(MemberInfo member)
        {
            if (member.GetCustomAttribute<AutoIncrementAttribute>() != null)
            {
                var type = member.GetMemberTypeInfo();
                if (NumericTypes.Contains(type))
                {
                    throw new Exception($"Type{type.Name} Cannot Be An AutoIncrement Member!");
                }

                IsAutoIncrement = true;
            }

            Member = member;
            IsPrimary = member.GetCustomAttribute<PrimaryKeyAttribute>() != null;
        }
    }
}
