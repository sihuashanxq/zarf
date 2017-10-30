using System.Linq;

namespace Zarf
{
    public interface IDbQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IDbQuery
    {
        DbContext Context { get; }
    }

    public interface IDbQuery
    {

    }
}
