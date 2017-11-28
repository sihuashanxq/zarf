using System.Linq;

namespace Zarf.Core
{
    public interface IInternalDbQuery
    {

    }

    public interface IInternalDbQuery<TEntity> : IInternalDbQuery, IQueryable<TEntity>, IOrderedQueryable<TEntity>
    {

    }
}
