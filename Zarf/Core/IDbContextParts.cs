using Zarf.Generators;
namespace Zarf.Core
{
    public interface IDbContextParts
    {
        ISQLGenerator CommandTextBuilder { get; }

        IDbEntityConnection EntityConnection { get; }

        IDbEntityCommandFacotry EntityCommandFacotry { get; }

        string ConnectionString { get; }
    }
}
