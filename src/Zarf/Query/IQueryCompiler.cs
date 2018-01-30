using System.Linq.Expressions;

namespace Zarf.Query
{
    public interface IQueryCompiler
    {
        Expression Compile(Expression query);
    }
}
