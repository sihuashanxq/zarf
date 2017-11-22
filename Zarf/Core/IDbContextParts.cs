using Zarf.Builders;
namespace Zarf.Core
{
    public interface IDbContextParts
    {
        ISqlTextBuilder CommandTextBuilder { get; }

        IDbEntityConnection EntityConnection { get; }

        IDbEntityCommandFacotry EntityCommandFacotry { get; }

        string ConnectionString { get; }
    }
}
