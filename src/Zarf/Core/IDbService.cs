using System;

namespace Zarf.Core
{
    public interface IDbService
    {
        IServiceProvider ServiceProvder { get; }

        IDbEntityConnection EntityConnection { get; }

        string ConnectionString { get; }
    }
}
