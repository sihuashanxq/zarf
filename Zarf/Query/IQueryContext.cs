using Zarf.Mapping;
using System.Linq.Expressions;
using Zarf.Query.Expressions;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Zarf.Query
{
    public interface IQueryContext
    {
        IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQuerySourceProvider QuerySourceProvider { get; }

        IProjectionScanner ProjectionScanner { get; }

        IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        IAliasGenerator Alias { get; }

        QueryExpression UpdateRefrenceSource(QueryExpression query);

        Dictionary<MemberInfo, object> SubQueryInstance { get; }
    }
}
