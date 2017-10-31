using System.Reflection;
using Zarf.Extensions;
using System.Data;
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
    }
}
