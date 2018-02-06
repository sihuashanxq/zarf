using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using Zarf.Metadata.Entities;
using Zarf.Metadata.DataAnnotations;

namespace Zarf.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type GetPropertyType(this MemberInfo memberInfo)
        {
            if (memberInfo.Is<FieldInfo>())
            {
                return memberInfo.Cast<FieldInfo>().FieldType;
            }

            if (memberInfo.Is<PropertyInfo>())
            {
                return memberInfo.Cast<PropertyInfo>().PropertyType;
            }

            return null;
        }

        public static bool IsStatic(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Constructor:
                    return memberInfo.Cast<ConstructorInfo>().IsStatic;
                case MemberTypes.Field:
                    return memberInfo.Cast<FieldInfo>().IsStatic;
                case MemberTypes.Method:
                    return memberInfo.Cast<MethodInfo>().IsStatic;
                case MemberTypes.Property:
                    var property = memberInfo.Cast<PropertyInfo>();
                    return (property.GetMethod ?? property.SetMethod).IsStatic;
                default:
                    return false;
            }
        }

        public static Type GetModelElementType(this Type typeInfo)
        {
            if (IsCollection(typeInfo))
            {
                var genericArguments = typeInfo.GetGenericArguments();
                if (genericArguments != null)
                {
                    return genericArguments.FirstOrDefault();
                }

                return typeInfo.GetElementType();
            }

            return typeInfo;
        }

        public static bool IsCollection(this Type type)
        {
            return !IsPrimtiveType(type) && (
                typeof(IEnumerable).IsAssignableFrom(type) ||
                typeof(IQuery).IsAssignableFrom(type) ||
                typeof(IEnumerator).IsAssignableFrom(type)
                );
        }

        public static bool IsPrimtiveType(this Type type)
        {
            return ReflectionUtil.SimpleTypes.Contains(type);
        }

        public static Table ToTable(this Type typeOfEntity)
        {
            var tableAttribute = typeOfEntity.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (tableAttribute == null)
            {
                return new Table(typeOfEntity.Name);
            }

            return new Table(tableAttribute.Name, tableAttribute.Schema.IsNullOrEmpty() ? "dbo" : tableAttribute.Schema);
        }

        public static Column ToColumn(this MemberInfo property)
        {
            return new Column(property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name);
        }

        public static TTarget As<TTarget>(this object o)
            where TTarget : class
        {
            return o as TTarget;
        }

        public static TTarget Cast<TTarget>(this object o)
        {
            return (TTarget)o;
        }

        public static bool Is<TTarget>(this object o)
        {
            return o is TTarget;
        }
    }
}
