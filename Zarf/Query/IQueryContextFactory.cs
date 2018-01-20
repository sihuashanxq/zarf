using Zarf.Core;
namespace Zarf.Queries
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext(IDbContextParts dbContextParts);

        IQueryContext CreateContext(
            IQueryMapper sourceProvider = null,
            IAliasGenerator generator = null,
            ISubQueryValueCache memValue = null,
            IDbContextParts dbContextParts = null);
    }
}
