using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    /// <summary>
    /// Map (x)=>   Expression
    /// </summary>
    public interface IQueryMapper
    {
        SelectExpression GetSelectExpression(ParameterExpression parameter);

        void AddSelectExpression(ParameterExpression parameter, SelectExpression select);
    }
}
