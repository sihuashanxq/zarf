using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zarf.Update
{
    public interface IDbModifyExecutor
    {
        int Execute(IEnumerable<EntityEntry> entries);

        Task<int> ExecuteAsync(IEnumerable<EntityEntry> entries);
    }
}
