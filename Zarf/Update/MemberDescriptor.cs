using System.Reflection;
using Zarf.Extensions;
using System.Data;
using Zarf.Entities;

namespace Zarf.Update
{
    public class MemberDescriptor
    {
        public bool IsPrimary { get; }

        public bool IsIncrement { get; }

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
            Member = member;
            IsPrimary = Member.GetCustomAttribute<PrimaryKeyAttribute>() != null;
            IsIncrement = Member.GetCustomAttribute<AutoIncrementAttribute>() != null;
        }
    }
}
