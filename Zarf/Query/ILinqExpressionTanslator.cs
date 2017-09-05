using System.Linq.Expressions;
using Zarf.Builders;

namespace Zarf.Query
{
    public interface ILinqExpressionTanslator
    {
        IQueryContext Context { get; }

        Expression Translate(Expression linqExpression);
    }
}
