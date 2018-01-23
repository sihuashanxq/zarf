using Zarf.Core;
namespace Zarf.Query
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext();
    }
}
