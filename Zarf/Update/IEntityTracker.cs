using System.Collections.Generic;
using System.Reflection;

namespace Zarf.Update
{
    public interface IEntityTracker
    {
        void TrackEntity<TEntity>(TEntity entity) where TEntity : class;

        void TrackEntity<TEntity>(TEntity entity, Dictionary<MemberInfo, object> memValues) where TEntity : class;

        bool IsTracked<TEntity>(TEntity entity) where TEntity : class;

        bool IsValueChanged<TEntity>(TEntity entity, MemberInfo member, object newMemValue) where TEntity : class;
    }
}
