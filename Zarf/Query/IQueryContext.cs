using Zarf.Mapping;

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
    }
}
