using System;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using System.Linq;
using Zarf.Mapping;
using System.Collections.Generic;

namespace Zarf.Update
{
    public class MemberDescriptor : IMemberDescriptor
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

        public bool IsPrimaryKey { get; }

        public bool IsAutoIncrement { get; }

        public bool IgnoreMapped { get; }

        public MemberInfo Member { get; }

        public bool IsConventionIdentity => Name == "Id";

        public string Name => Member.Name;

        public string RefrenceForeignKey { get; }

        public Type MemberType { get; }

        public IEnumerable<Attribute> Attributes { get; }

        public MemberDescriptor(MemberInfo member)
        {
            Member = member;
            Attributes = Member.GetCustomAttributes();
            MemberType = member.GetPropertyType();
            IsPrimaryKey = Attributes.FirstOrDefault(item => item is PrimaryKeyAttribute) != null;
            RefrenceForeignKey = Attributes.OfType<ForeignKeyAttribute>()?.FirstOrDefault()?.Name ?? Name;
            IsAutoIncrement = NumericTypes.Contains(MemberType) && Attributes.OfType<AutoIncrementAttribute>().FirstOrDefault() != null;
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
