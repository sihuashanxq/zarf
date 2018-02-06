using System;
using System.Reflection;
using Zarf.Extensions;
using System.Linq;
using System.Collections.Generic;
using Zarf.Metadata.DataAnnotations;

namespace Zarf.Metadata.Descriptors
{
    public class MemberDescriptor : IMemberDescriptor
    {
        public bool IsPrimaryKey { get; }

        public bool IsAutoIncrement { get; }

        public bool IgnoreMapped { get; }

        public MemberInfo Member { get; }

        public bool IsConventionIdentity => Name == "Id";

        public string Name => Member.Name;

        public Type MemberType { get; }

        public IEnumerable<Attribute> Attributes { get; }

        public MemberDescriptor(MemberInfo member)
        {
            Member = member;
            Attributes = Member.GetCustomAttributes();
            MemberType = member.GetPropertyType();
            IsPrimaryKey = Attributes.FirstOrDefault(item => item is PrimaryKeyAttribute) != null;
            IsAutoIncrement = ReflectionUtil.NumbericTypes.Contains(MemberType) && Attributes.OfType<AutoIncrementAttribute>().FirstOrDefault() != null;
        }

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
                var field = Member.As<FieldInfo>();
                field.SetValue(entity, Convert.ChangeType(value, field.FieldType));
            }
            else
            {
                var property = Member.As<PropertyInfo>();
                property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
            }
        }
    }
}
