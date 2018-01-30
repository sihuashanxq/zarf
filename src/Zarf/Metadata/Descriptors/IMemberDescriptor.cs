using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zarf.Metadata.Descriptors
{
    public interface IMemberDescriptor
    {
        bool IsPrimaryKey { get; }

        bool IsConventionIdentity { get; }

        bool IsAutoIncrement { get; }

        string Name { get; }

        string RefrenceForeignKey { get; }

        MemberInfo Member { get; }

        Type MemberType { get; }

        IEnumerable<Attribute> Attributes { get; }

        object GetValue(object obj);

        void SetValue(object obj, object value);
    }
}
