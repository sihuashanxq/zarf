using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    }
}
