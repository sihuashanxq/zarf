using Zarf.Core;
using Zarf.Mapping;
namespace Zarf.Query
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext(IDbContextParts dbContextParts);

        IQueryContext CreateContext(
            IMemberAccessMapper sourceMappingProvider = null,
            IQueryColumnOrdinalMapper mappingProvider = null,
            IPropertyNavigationContext navigationContext = null,
            ILambdaParameterMapper sourceProvider = null,
            IProjectionScanner scanner = null,
            IAliasGenerator generator = null,
            IMemberValueCache memValue = null,
            IDbContextParts dbContextParts = null);
    }
}
