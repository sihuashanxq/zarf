using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public interface IEntityProjectionMapping
    {
        Expression Source { get; }

        Expression Expression { get; }

        int Ordinal { get; }
    }
}
