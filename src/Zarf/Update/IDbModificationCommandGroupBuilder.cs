using System.Collections.Generic;

namespace Zarf.Update
{
    public interface IDbModificationCommandGroupBuilder
    {
        List<DbModificationCommandGroup> Build(IEnumerable<EntityEntry> entries);
    }
}
