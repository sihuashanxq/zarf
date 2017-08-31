using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public interface IMapping
    {
        Expression Source { get; }

        Expression Expression { get; }

        int Ordinal { get; }
    }
}
