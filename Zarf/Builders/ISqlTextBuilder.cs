using System.Linq.Expressions;

namespace Zarf.Builders
{
    public interface ISqlTextBuilder
    {
        string Build(Expression expression);
    }
}
