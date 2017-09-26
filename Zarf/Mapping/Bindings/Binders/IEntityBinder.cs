using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IEntityBinder
    {
        Expression Bind(IBindingContext bindingContext);
    }
}
