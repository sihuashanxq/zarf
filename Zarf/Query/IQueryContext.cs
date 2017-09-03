namespace Zarf.Query
{
    public interface IQueryContext
    {
        IEntityMemberSourceMappingProvider EntityMemberMappingProvider { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQuerySourceProvider QuerySourceProvider { get; }

        IRefrenceProjectionFinder ProjectionFinder { get; }

        IAliasGenerator Alias { get; }
    }
}
