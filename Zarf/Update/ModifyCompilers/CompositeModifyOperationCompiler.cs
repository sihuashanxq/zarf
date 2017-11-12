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

        public CompositeModifyOperationCompiler()
        {
            _syncRoot = new object();
            _trackEntityMemValues = new Dictionary<object, Dictionary<MemberInfo, object>>();

            InternalCompilers = new Dictionary<EntityState, IModifyOperationCompiler>
            {
                [EntityState.Insert] = new InsertOperationCompiler(),
                [EntityState.Update] = new UpdateOperationCompiler(_trackEntityMemValues),
                [EntityState.Delete] = new DeleteOperationCompiler()
            };
        }

        public override void TrackEntity<TEntity>(TEntity entity)
        {
            lock (_syncRoot)
            {
                var eType = typeof(TEntity);
                var values = new Dictionary<MemberInfo, object>();

                foreach (var item in eType.GetProperties().Where(item => ReflectionUtil.SimpleTypes.Contains(item.PropertyType)))
                {
                    values[item] = item.GetValue(entity);
                }

                foreach (var item in eType.GetFields().Where(item => ReflectionUtil.SimpleTypes.Contains(item.FieldType)))
                {
                    values[item] = item.GetValue(entity);
                }

                _trackEntityMemValues[entity] = values;
            }
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
