using System.Linq.Expressions;

namespace Zarf.Query
{
    public interface IQuerySourceProvider
    {
        Expression GetSource(Expression expression);

        void AddSource(Expression expression, Expression source);
    }
}
