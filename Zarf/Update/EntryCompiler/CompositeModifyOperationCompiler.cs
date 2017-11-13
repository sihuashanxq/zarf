using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public class CompositeModifyOperationCompiler : ModifyOperationCompiler
    {
        protected Dictionary<EntityState, IModifyOperationCompiler> InternalCompilers { get; }

        protected Dictionary<object, Dictionary<MemberInfo, object>> _trackEntityMemValues;

        protected object _syncRoot;

        public CompositeModifyOperationCompiler(IEntityTracker tracker)
        {
            _syncRoot = new object();
            _trackEntityMemValues = new Dictionary<object, Dictionary<MemberInfo, object>>();

            InternalCompilers = new Dictionary<EntityState, IModifyOperationCompiler>
            {
                [EntityState.Insert] = new InsertOperationCompiler(),
                [EntityState.Update] = new UpdateOperationCompiler(tracker),
                [EntityState.Delete] = new DeleteOperationCompiler()
            };
        }

        public override DbModifyCommand Compile(EntityEntry entity, MemberDescriptor primary)
        {
            if (primary == null)
            {
                throw new KeyNotFoundException($"Type {entity.Entity.GetType().Name} should be set an member as primary!");
            }

            if (InternalCompilers.TryGetValue(entity.State, out IModifyOperationCompiler internalCompiler))
            {
                return internalCompiler.Compile(entity, primary);
            }

            return null;
        }
    }
}
