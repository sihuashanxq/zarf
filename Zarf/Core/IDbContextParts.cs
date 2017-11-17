using Zarf.Builders;

namespace Zarf.Core
{
    public interface IDbContextParts
    {
        ISqlTextBuilder SqlBuilder { get; }

        IDbConnectionWrapper DbConnection { get; }

        IDbCommandFacotry CommandFacotry { get; }
    }
}
