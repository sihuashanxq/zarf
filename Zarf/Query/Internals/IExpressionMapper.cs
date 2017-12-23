using System.Linq.Expressions;

namespace Zarf.Query.Internals
{
    public interface IExpressionMapper
    {
        void Map(Expression key, Expression value);

        Expression GetMappedExpression(Expression key);
    }
}
