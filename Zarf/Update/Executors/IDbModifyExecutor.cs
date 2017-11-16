using System.Collections.Generic;

namespace Zarf.Update
{
    public interface IDbModifyExecutor
    {
        int Execute(IEnumerable<EntityEntry> entries);
    }
}
