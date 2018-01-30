using System;

namespace Zarf.Core
{
    public interface IDbEntityTransaction : IDisposable
    {
        Guid Id { get; }

        void Rollback();

        void Commit();

        new void Dispose();
    }
}
