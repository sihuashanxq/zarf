using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Update.Compilers
{

    public class CompositeModifyOperationCompiler : ModifyOperationCompiler
    {
        protected static Dictionary<EntityState, IModifyOperationCompiler> InternalCompilers { get; }

        static CompositeModifyOperationCompiler()
        {
            InternalCompilers = new Dictionary<EntityState, IModifyOperationCompiler>
            {
                [EntityState.Insert] = new InsertOperationCompiler(),
                [EntityState.Update] = new UpdateOperationCompiler(),
                [EntityState.Delete] = new DeleteOperationCompiler()
            };
        }

        public override DbModifyCommand Compile(EntityEntry entity, MemberDescriptor identity)
        {
            if (identity == null)
            {
                identity = entity.Primary ?? entity.Increment ?? entity.Members.FirstOrDefault(item => item.Member.Name.ToLower() == "id");
            }

            if (identity == null)
            {
                throw new KeyNotFoundException($"Type {entity.Entity.GetType().Name} should be set an member as primary!");
            }

            if (InternalCompilers.TryGetValue(entity.State, out IModifyOperationCompiler internalCompiler))
            {
                return internalCompiler.Compile(entity, identity);
            }

            return null;
        }
    }
}
