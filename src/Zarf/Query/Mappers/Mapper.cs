using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Infrastructure;
using Zarf.Query.Expressions;

namespace Zarf.Query.Mappers
{
    public class Mapper<TKey, TValue> : IMapper<TKey, TValue>
        where TKey : Expression
    {
        private ConcurrentDictionary<TKey, TValue> _keyValues { get; }

        public IEnumerable<TKey> Keys => _keyValues.Keys;

        public IEnumerable<TValue> Values => _keyValues.Values;

        public TValue this[TKey key]
        {
            get => GetValue(key);
            set => Map(key, value);
        }

        public Mapper()
        {
            _keyValues = new ConcurrentDictionary<TKey, TValue>(new ExpressionEqualityComparer());
        }

        public TValue GetValue(TKey key)
        {
            if (key == null)
            {
                return default(TValue);
            }

            return _keyValues.TryGetValue(key, out TValue v) ? v : default(TValue);
        }

        public void Map(TKey key, TValue v)
        {
            if (key != null)
            {
                _keyValues.AddOrUpdate(key, v, (k, _) => v);
            }
        }

        public TKey GetKey(TValue value)
        {
            foreach (var item in _keyValues)
            {
                if (value.Equals(item.Value))
                {
                    return item.Key;
                }
            }

            return default(TKey);
        }
    }
}
