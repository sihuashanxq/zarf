using System.Reflection;
using System.Collections.Generic;

using Zarf.Mapping;
using Zarf.Query.Expressions;
using System;
using System.Data;

namespace Zarf.Query
{
    public class QueryContext : IQueryContext
    {
        public IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        public IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        public IPropertyNavigationContext PropertyNavigationContext { get; }

        public IQuerySourceProvider QuerySourceProvider { get; }

        public IProjectionScanner ProjectionScanner { get; }

        public IAliasGenerator Alias { get; }

        public Dictionary<MemberInfo, object> SubQueryInstance { get; set; }

        public QueryContext(
            IEntityMemberSourceMappingProvider memberMappingProvider,
            IEntityProjectionMappingProvider projectionMappingProvider,
            IPropertyNavigationContext navigationContext,
            IQuerySourceProvider sourceProvider,
            IProjectionScanner projectionFinder,
            IAliasGenerator aliasGenerator
            )
        {
            EntityMemberMappingProvider = memberMappingProvider;
            ProjectionMappingProvider = projectionMappingProvider;
            PropertyNavigationContext = navigationContext;
            QuerySourceProvider = sourceProvider;
            ProjectionScanner = projectionFinder;
            Alias = aliasGenerator;
            SubQueryInstance = new Dictionary<MemberInfo, object>();
        }

        public QueryExpression UpdateRefrenceSource(QueryExpression query)
        {
            EntityMemberMappingProvider.UpdateExpression(query.SubQuery, query);
            return query;
        }
    }
}
