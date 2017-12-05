using System.Linq.Expressions;

namespace Zarf.Query
{
    /// <summary>
    /// Map (x)=>   Expression
    /// </summary>
    public interface ILambdaParameterMapper
    {
        Expression GetMappedExpression(ParameterExpression parameter);

        void Map(ParameterExpression parameter, Expression mapExpression);
    }
}
