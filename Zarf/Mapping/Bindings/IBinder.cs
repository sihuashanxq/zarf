using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBinder
    {
        Expression Bind(IBindingContext bindingContext);
    }
}
