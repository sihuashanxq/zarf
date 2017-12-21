using Zarf.Core;
using Zarf.Mapping;
namespace Zarf.Query
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext(IDbContextParts dbContextParts);

        IQueryContext CreateContext(
            IQueryColumnOrdinalMapper mappingProvider = null,
            IPropertyNavigationContext navigationContext = null,
            IQueryMapper sourceProvider = null,
            IAliasGenerator generator = null,
            IMemberValueCache memValue = null,
            IDbContextParts dbContextParts = null);
    }
}
