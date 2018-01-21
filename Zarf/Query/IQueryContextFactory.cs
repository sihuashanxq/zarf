using Zarf.Core;
namespace Zarf.Queries
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext();
    }
}
