using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf.Update.Commands
{
    public class DbModifyCommand
    {
        public Table Table { get; }

        public EntityEntry Entity { get; }

        public DbModifyCommand(
            EntityEntry entity)
        {
            Entity = entity;
            Table = Entity.Type.ToTable();
        }
    }
}
