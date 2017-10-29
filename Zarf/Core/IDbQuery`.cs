using System.Linq;

namespace Zarf
{
    public interface IDbQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IDbQuery
    {

    }

    public interface IDbQuery
    {

    }
}
