using Zarf.Builders;
using System;
namespace Zarf.Core
{
    public interface IDbContextParts
    {
        ISqlTextBuilder CommandTextBuilder { get; }

        IDbEntityConnection EntityConnection { get; }

        IDbEntityCommandFacotry EntityCommandFacotry { get; }
    }
}
