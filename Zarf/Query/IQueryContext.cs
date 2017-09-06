using Zarf.Mapping;
using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public interface IQueryContext
    {
        IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQuerySourceProvider QuerySourceProvider { get; }

        IRefrenceProjectionFinder ProjectionFinder { get; }

        IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        IAliasGenerator Alias { get; }

        QueryExpression UpdateRefrenceSource(QueryExpression query);
    }
}
