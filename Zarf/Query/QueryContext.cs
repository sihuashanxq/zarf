using System.Reflection;
using System.Collections.Generic;

using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public interface IMemberValueCache
    {
        void SetValue(MemberInfo mem, object value);

        object GetValue(MemberInfo mem);
    }

    public class MemberValueCache : IMemberValueCache
    {
        private Dictionary<MemberInfo, object> _memValues = new Dictionary<MemberInfo, object>();

        public object GetValue(MemberInfo mem)
        {
            if (_memValues.TryGetValue(mem, out object value))
            {
                return value;
            }

            return null;
        }

        public void SetValue(MemberInfo mem, object value)
        {
            _memValues[mem] = value;
        }
    }

    public class QueryContext : IQueryContext
    {
        public IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        public IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        public IPropertyNavigationContext PropertyNavigationContext { get; }

        public IQuerySourceProvider QuerySourceProvider { get; }

        public IProjectionScanner ProjectionScanner { get; }

        public IAliasGenerator Alias { get; }

        public IMemberValueCache MemberValueCache { get; }

        public QueryContext(
            IEntityMemberSourceMappingProvider memberMappingProvider,
            IEntityProjectionMappingProvider projectionMappingProvider,
            IPropertyNavigationContext navigationContext,
            IQuerySourceProvider sourceProvider,
            IProjectionScanner projectionFinder,
            IAliasGenerator aliasGenerator,
            IMemberValueCache memValueCache
            )
        {
            EntityMemberMappingProvider = memberMappingProvider;
            ProjectionMappingProvider = projectionMappingProvider;
            PropertyNavigationContext = navigationContext;
            QuerySourceProvider = sourceProvider;
            ProjectionScanner = projectionFinder;
            Alias = aliasGenerator;
            MemberValueCache = memValueCache;
        }

        public QueryExpression UpdateRefrenceSource(QueryExpression query)
        {
            EntityMemberMappingProvider.UpdateExpression(query.SubQuery, query);
            return query;
        }
    }
}
