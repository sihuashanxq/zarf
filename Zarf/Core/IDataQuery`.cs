using System.Linq;

namespace Zarf
{
    public interface IDataQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IDataQuery
    {

    }

    public interface IDataQuery
    {

    }
}
