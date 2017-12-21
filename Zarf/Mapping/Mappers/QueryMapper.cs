using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class QueryMapper : IQueryMapper
    {
        protected virtual Dictionary<ParameterExpression, QueryExpression> Queries { get; set; }

        public QueryMapper()
        {
            Queries = new Dictionary<ParameterExpression, QueryExpression>();
        }

        public void MapQuery(ParameterExpression parameter, QueryExpression query)
        {
            if (parameter == null || query == null)
            {
                return;
            }

            Queries[parameter] = query;
        }

        public QueryExpression GetMappedQuery(ParameterExpression parameter)
        {
            return Queries.TryGetValue(parameter, out QueryExpression query) ? query : null;
        }
    }
}
