using System.Linq.Expressions;
using System.Collections.Generic;

namespace Zarf.Query.Internals
{
    public interface IQueryProjectionMapper
    {
        void Map(Expression projection, Expression mappedProjection);

        Expression GetMappedProjection(Expression projection);

        IEnumerable<KeyValuePair<Expression, Expression>> GetAllProjections();
    }
}
