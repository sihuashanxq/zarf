using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class QueryMapper : IQueryMapper
    {
        protected  Dictionary<ParameterExpression, SelectExpression> Mappers { get; set; }

        public QueryMapper()
        {
            Mappers = new Dictionary<ParameterExpression, SelectExpression>();
        }

        public void AddSelectExpression(ParameterExpression parameter, SelectExpression select)
        {
            if (parameter == null || select == null)
            {
                return;
            }

            Mappers[parameter] = select;
        }

        public SelectExpression GetSelectExpression(ParameterExpression parameter)
        {
            return Mappers.TryGetValue(parameter, out SelectExpression select) ? select : null;
        }
    }
}
