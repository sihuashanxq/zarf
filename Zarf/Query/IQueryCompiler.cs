using System.Linq.Expressions;

namespace Zarf.Queries
{
    public interface IQueryCompiler
    {
        Expression Compile(Expression query);
    }
}
