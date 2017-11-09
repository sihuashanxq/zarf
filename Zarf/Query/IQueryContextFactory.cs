using Zarf.Core;
using Zarf.Mapping;
namespace Zarf.Query
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext(IDbContextParts dbContextParts);

        IQueryContext CreateContext(
            IEntityMemberSourceMappingProvider sourceMappingProvider = null,
            IEntityProjectionMappingProvider mappingProvider = null,
            IPropertyNavigationContext navigationContext = null,
            IQuerySourceProvider sourceProvider = null,
            IProjectionScanner scanner = null,
            IAliasGenerator generator = null,
            IMemberValueCache memValue = null,
            IDbContextParts dbContextParts = null);
    }
}
