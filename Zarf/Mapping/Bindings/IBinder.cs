using System;
using System.Data;

namespace Zarf.Mapping.Bindings
{
    public interface IBinder
    {
        Func<IDataReader, TEntity> Bind<TEntity>(IBindingContext bindingContext);
    }
}
