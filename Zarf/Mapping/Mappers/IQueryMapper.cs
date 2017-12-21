using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query
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
