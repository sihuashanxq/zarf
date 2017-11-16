using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Zarf.Update
{
    public class EntityTracker : IEntityTracker
    {
        protected ConcurrentDictionary<object, ConcurrentDictionary<MemberInfo, object>> _trackEntityMemValues;

        public EntityTracker()
        {
            _trackEntityMemValues = new ConcurrentDictionary<object, ConcurrentDictionary<MemberInfo, object>>();
        }

        public bool IsTracked<TEntity>(TEntity entity) where TEntity : class
        {
            return _trackEntityMemValues.ContainsKey(entity);
        }

        public bool IsValueChanged<TEntity>(TEntity entity, MemberInfo member, object newMemValue) where TEntity : class
        {
            if (!IsTracked(entity))
            {
                return true;
            }

            if (_trackEntityMemValues.TryGetValue(entity, out var memValues))
            {
                if (memValues.TryGetValue(member, out var originValue))
                {
                    if (newMemValue == null && originValue == null)
                    {
                        return false;
                    }

                    if (newMemValue != null && newMemValue.Equals(originValue))
                    {
                        return false;
                    }

                    if (originValue != null && originValue.Equals(newMemValue))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void TrackEntity<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("tracked entity is null!");
            }

            var memValues = new Dictionary<MemberInfo, object>();
            var eType = typeof(TEntity);

            foreach (var item in eType.GetProperties().Where(item => ReflectionUtil.SimpleTypes.Contains(item.PropertyType)))
            {
                memValues[item] = item.GetValue(entity);
            }

            foreach (var item in eType.GetFields().Where(item => ReflectionUtil.SimpleTypes.Contains(item.FieldType)))
            {
                memValues[item] = item.GetValue(entity);
            }

            TrackEntity(entity, memValues);
        }

        public void TrackEntity<TEntity>(TEntity entity, Dictionary<MemberInfo, object> memValues) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("tracked entity is null!");
            }

            if (memValues == null)
            {
                throw new ArgumentNullException("tracked member values is null!");
            }

            var conCurrentMemValues = new ConcurrentDictionary<MemberInfo, object>(memValues);

            _trackEntityMemValues.AddOrUpdate(entity, conCurrentMemValues, (key, trackedValues) =>
            {
                foreach (var item in conCurrentMemValues)
                {
                    trackedValues.AddOrUpdate(item.Key, item.Value, (k, o) => item.Value);
                }

                return trackedValues;
            });
        }

        public void Clear()
        {
            _trackEntityMemValues.Clear();
        }
    }
}
