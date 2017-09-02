using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;
using System.Linq;

namespace Zarf.Query
{
    public class QueryContext : IQueryContext
    {
        public List<Expression> Projections { get; set; }

        public Dictionary<ParameterExpression, QueryExpression> QuerySource { get; set; }

        public Dictionary<MemberInfo, QueryExpression> MemberSource { get; set; }

        public Dictionary<MemberInfo, QueryExpression> Includes = new Dictionary<MemberInfo, QueryExpression> { };

        public Dictionary<MemberInfo, Expression> IncludesCondtion = new Dictionary<MemberInfo, Expression>();

        public Dictionary<MemberInfo, List<Expression>> IncludesCondtionParameter = new Dictionary<MemberInfo, List<Expression>>();

        public Dictionary<MemberInfo, object> IncludesMemberInstances = new Dictionary<MemberInfo, object>();

        public QueryContext()
        {
            QuerySource = new Dictionary<ParameterExpression, QueryExpression>();
            MemberSource = new Dictionary<MemberInfo, QueryExpression>();
            Projections = new List<Expression>();
        }

        private int _aliasIndex = 0;

        public string CreateAlias()
        {
            return "T" + _aliasIndex++;
        }

        public QueryExpression UpdateRefrenceSource(QueryExpression query)
        {
            foreach (var kv in MemberSource.ToList())
            {
                if (kv.Value == query.SubQuery)
                {
                    MemberSource[kv.Key] = query;
                }
            }

            return query;
        }

        public IEntityMemberMappingProvider EntityMemberMappingProvider { get; }

        public IPropertyNavigationContext PropertyNavigationContext { get; }

        public IQuerySourceProvider QuerySourceProvider { get; }

        public IProjectionFinder ProjectionFinder { get; }

        public IAliasGenerator AliasGenerator { get; }

        public QueryContext(
            IEntityMemberMappingProvider memberMappingProvider,
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
