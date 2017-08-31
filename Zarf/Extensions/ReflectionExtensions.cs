using System;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace Zarf.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type GetMemberInfoType(this MemberInfo memberInfo)
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
            if (memberInfo.Is<FieldInfo>())
            {
                return memberInfo.Cast<FieldInfo>().IsStatic;
            }

            if (memberInfo.Is<ConstructorInfo>())
            {
                return memberInfo.Cast<ConstructorInfo>().IsStatic;
            }

            if (memberInfo.Is<MethodInfo>())
            {
                return memberInfo.Cast<MethodInfo>().IsStatic;
            }

            if (memberInfo.Is<PropertyInfo>())
            {
                var property = memberInfo.Cast<PropertyInfo>();
                if (property.GetMethod != null)
                {
                    return property.GetMethod.IsStatic;
                }

                return property.SetMethod.IsStatic;
            }

            return false;
        }

        public static Type GetElementTypeInfo(this Type type)
        {
            if (typeof(IQueryable).IsAssignableFrom(type) && type.GetTypeInfo().IsGenericType)
            {
                var elementType = type.GetElementType();
                if (elementType == null)
                {
                    return type.GetGenericArguments()[0];
                }

                return elementType;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetTypeInfo().IsGenericType)
            {
                var elementType = type.GetElementType();
                if (elementType == null)
                {
                    return type.GetGenericArguments()[0];
                }

                return elementType;
            }
            return type;
        }

        public static T As<T>(this object o)
            where T : class
            => o as T;

        public static T Cast<T>(this object o)
            => (T)o;

        public static bool Is<T>(this object o)
            => o is T;
    }
}
