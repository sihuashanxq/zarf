namespace Zarf.Query
{
    public interface IQueryContext
    {
        IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQuerySourceProvider QuerySourceProvider { get; }

        IProjectionFinder ProjectionFinder { get; }

        IAliasGenerator AliasGenerator { get; }
    }
}
