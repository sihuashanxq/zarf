using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Core
{
    public interface IDbEntityTransaction
    {
        Guid Id { get; }

        void Rollback();

        void Commit();
    }
}
