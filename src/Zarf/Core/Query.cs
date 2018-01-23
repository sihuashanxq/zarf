using Zarf.Core.Internals;

namespace Zarf.Core
{
    public abstract class Query
    {
        internal abstract IInternalQuery InternalQuery { get; }
    }
}
