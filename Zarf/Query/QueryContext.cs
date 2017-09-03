using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class QueryContext : IQueryContext
    {
        public Dictionary<MemberInfo, QueryExpression> Includes = new Dictionary<MemberInfo, QueryExpression> { };

        public Dictionary<MemberInfo, Expression> IncludesCondtion = new Dictionary<MemberInfo, Expression>();

        public Dictionary<MemberInfo, List<Expression>> IncludesCondtionParameter = new Dictionary<MemberInfo, List<Expression>>();

        public Dictionary<MemberInfo, object> IncludesMemberInstances = new Dictionary<MemberInfo, object>();

        public QueryExpression UpdateRefrenceSource(QueryExpression query)
        {
            EntityMemberMappingProvider.UpdateExpression(query.SubQuery, query);
            return query;
        }

        public IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        public IPropertyNavigationContext PropertyNavigationContext { get; }

        public IQuerySourceProvider QuerySourceProvider { get; }

        public IProjectionFinder ProjectionFinder { get; }

        public IAliasGenerator AliasGenerator { get; }

        public QueryContext(
            IEntityMemberSourceMappingProvider memberMappingProvider,
            IPropertyNavigationContext navigationContext,
            IQuerySourceProvider sourceProvider,
            IProjectionFinder projectionFinder,
            IAliasGenerator aliasGenerator
            )
        {
            EntityMemberMappingProvider = memberMappingProvider;
            PropertyNavigationContext = navigationContext;
            QuerySourceProvider = sourceProvider;
            ProjectionFinder = projectionFinder;
            AliasGenerator = aliasGenerator;
        }
    }
}
