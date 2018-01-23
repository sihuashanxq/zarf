using System.Collections.Generic;

namespace Zarf.Query.Internals
{
    public class QueryValueCache : IQueryValueCache
    {
        private Dictionary<QueryEntityModel, object> _memValues = new Dictionary<QueryEntityModel, object>();

        public object GetValue(QueryEntityModel queryModel)
        {
            if (_memValues.TryGetValue(queryModel, out object value))
            {
                return value;
            }

            return null;
        }

        public void SetValue(QueryEntityModel queryModel, object value)
        {
            _memValues[queryModel] = value;
        }
    }
}
