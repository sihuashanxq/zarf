using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query
{
    public class LambdaParameterMapper : ILambdaParameterMapper
    {
        protected virtual Dictionary<ParameterExpression, Expression> Parameters { get; set; }

        public LambdaParameterMapper()
        {
            Parameters = new Dictionary<ParameterExpression, Expression>();
        }

        public void Map(ParameterExpression parameter, Expression mapExpression)
        {
            Parameters[parameter] = mapExpression;
        }

        public Expression GetMappedExpression(ParameterExpression parameter)
        {
            return Parameters.TryGetValue(parameter, out Expression mapExpression) ? mapExpression : null;
        }
    }
}
