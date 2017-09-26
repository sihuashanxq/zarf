using System;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace Zarf.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type GetMemberTypeInfo(this MemberInfo memberInfo)
        {
            if (memberInfo.Is<FieldInfo>())
            {
                return memberInfo.Cast<FieldInfo>().FieldType;
            }

            if (memberInfo.Is<PropertyInfo>())
            {
                return memberInfo.Cast<PropertyInfo>().PropertyType;
            }

            throw new NotImplementedException($"{memberInfo.Name} is not a field or property");
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

        public static Type GetCollectionElementType(this Type typeInfo)
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
            if (type.IsPrimtiveType())
            {
                return false;
            }

            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsPrimtiveType(this Type type)
        {
            return ReflectionUtil.SimpleTypes.Contains(type);
        }

        public static T As<T>(this object o)
            where T : class
        {
            return o as T;
        }

        public static T Cast<T>(this object o)
        {
            return (T)o;
        }

        public static bool Is<T>(this object o)
        {
            return o is T;
        }
    }
}
