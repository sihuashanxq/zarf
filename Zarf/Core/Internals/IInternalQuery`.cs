using System.Linq;

namespace Zarf.Core.Internals
{
    public interface IInternalQuery
    {

    }

    public interface IInternalQuery<TEntity> : IInternalQuery, IQueryable<TEntity>, IOrderedQueryable<TEntity>
    {

    }
}
