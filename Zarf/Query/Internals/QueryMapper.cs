using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries
{
    public class QueryMapper : IQueryMapper
    {
        protected  Dictionary<ParameterExpression, QueryExpression> Mappers { get; set; }

        public QueryMapper()
        {
            Mappers = new Dictionary<ParameterExpression, QueryExpression>();
        }

        public void MapQuery(ParameterExpression parameter, QueryExpression query)
        {
            if (parameter == null || query == null)
            {
                return;
            }

            Mappers[parameter] = query;
        }

        public QueryExpression GetMappedQuery(ParameterExpression parameter)
        {
            return Mappers.TryGetValue(parameter, out QueryExpression query) ? query : null;
        }
    }
}
