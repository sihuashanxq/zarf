using System.Collections.Generic;
using Zarf.Update.Commands;

namespace Zarf.Update
{
    public interface IModifyOperationCompiler
    {
        IEnumerable<DbModifyCommand> Compile(DbModifyOperation modifyOperation);

        DbModifyCommand Compile(EntityEntry entry, MemberDescriptor identity);

        void TrackEntity<TEntity>(TEntity entity);
    }
}
