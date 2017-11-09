using Zarf.Builders;

namespace Zarf.Core
{
    public interface IDbContextParts
    {
        ISqlTextBuilder SqlBuilder { get; }

        IDbCommandFacotry CommandFacotry { get; }
    }
}
