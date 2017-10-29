using System;

namespace Zarf.Entities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AutoIncrementAttribute : Attribute
    {
    }
}
