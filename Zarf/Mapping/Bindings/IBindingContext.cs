using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        int GetExpressionOrdinal(Expression node);
    }
}
