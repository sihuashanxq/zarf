using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query.Mappers
{
    public interface IMapper<TKey, TValue>
           where TKey : Expression
    {
        void Map(TKey key, TValue v);

        TValue GetValue(TKey key);

        TKey GetKey(TValue value);

        TValue this[TKey key] { get; set; }

        IEnumerable<TKey> Keys { get; }

        IEnumerable<TValue> Values { get; }
    }

}
