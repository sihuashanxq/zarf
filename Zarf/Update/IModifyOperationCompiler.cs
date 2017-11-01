using System.Collections.Generic;
using System.Linq;

namespace Zarf.Update
{
    public interface IModifyOperationCompiler
    {
        IEnumerable<DbModifyCommand> Compile(DbModifyOperation modifyOperation);

        DbModifyCommand Compile(EntityEntry entry, MemberDescriptor operationKey);
    }

    public abstract class ModifyOperationCompiler : IModifyOperationCompiler
    {
        public virtual IEnumerable<DbModifyCommand> Compile(DbModifyOperation modifyOperation)
        {
            foreach (var entry in modifyOperation.Entities)
            {
                var modifyCommand = Compile(entry, modifyOperation.OperationKey);
                if (modifyCommand != null)
                {
                    yield return modifyCommand;
                }
            }
        }

        public abstract DbModifyCommand Compile(EntityEntry entry, MemberDescriptor operationKey);
    }

    public class CompositeModifyOperationCompiler : ModifyOperationCompiler
    {
        protected static Dictionary<EntityState, IModifyOperationCompiler> InternalCompilers { get; }

        static CompositeModifyOperationCompiler()
        {
            InternalCompilers = new Dictionary<EntityState, IModifyOperationCompiler>
            {
                [EntityState.Add] = new AddOperationCompiler(),
                [EntityState.Update] = new UpdateOperationCompiler(),
                [EntityState.Delete] = new DeleteOperationCompiler()
            };
        }

        public override DbModifyCommand Compile(EntityEntry entity, MemberDescriptor operationKey)
        {
            if (operationKey == null)
            {
                operationKey = entity.Primary ?? entity.Increment ?? entity.Members.FirstOrDefault(item => item.Member.Name.ToLower() == "id");
            }

            if (operationKey == null)
            {
                throw new KeyNotFoundException($"Type {entity.Entity.GetType().Name} should be set an member as primary!");
            }

            if (InternalCompilers.TryGetValue(entity.State, out IModifyOperationCompiler internalCompiler))
            {
                return internalCompiler.Compile(entity, operationKey);
            }

            return null;
        }
    }

    //TODO TABLE 1 TABLE 2 TOTABLE
    public class AddOperationCompiler : ModifyOperationCompiler
    {
        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor operationKey)
        {

        }
    }

    public class UpdateOperationCompiler : ModifyOperationCompiler
    {
        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor operationKey)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DeleteOperationCompiler : ModifyOperationCompiler
    {
        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor operationKey)
        {
            throw new System.NotImplementedException();
        }
    }
}
