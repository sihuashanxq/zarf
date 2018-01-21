using System.Collections.Generic;
using Zarf.Mapping;
using Zarf.Core;
using Zarf.Queries.Internals;
using Zarf.Entities;

namespace Zarf.Queries
{
    public interface ISubQueryValueCache
    {
        void SetValue(QueryEntityModel queryModel, object value);

        object GetValue(QueryEntityModel queryModel);
    }

    public class SubQueryValueCache : ISubQueryValueCache
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

    public class QueryContext : IQueryContext
    {
        public IQueryMapper QueryMapper { get; }

        public IAliasGenerator Alias { get; }

        public ISubQueryValueCache MemberValueCache { get; }

        public IQueryProjectionMapper ColumnCaching { get; }

        public MemberBindingMapper MemberBindingMapper { get; }

        public ProjectionOwnerMapper ProjectionOwner { get; }

        public QueryModelMapper QueryModelMapper { get; }

        public IQueryProjectionMapper ExpressionMapper { get; }

        public QueryContext(
            IQueryMapper sourceProvider,
            IAliasGenerator aliasGenerator,
            ISubQueryValueCache memValueCache
            )
        {
            QueryMapper = sourceProvider;
            Alias = aliasGenerator;
            MemberValueCache = memValueCache;
            ColumnCaching = new QueryProjectionMapper();
            MemberBindingMapper = new MemberBindingMapper();
            ProjectionOwner = new ProjectionOwnerMapper();
            QueryModelMapper = new QueryModelMapper();
            ExpressionMapper = new QueryProjectionMapper();
        }
    }
}
