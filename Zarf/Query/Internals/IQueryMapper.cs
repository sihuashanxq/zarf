using System.Linq.Expressions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries
{
    /// <summary>
    /// Map (x)=>   Expression
    /// </summary>
    public interface IQueryMapper
    {
        QueryExpression GetMappedQuery(ParameterExpression parameter);

        void MapQuery(ParameterExpression parameter, QueryExpression query);
    }
}
