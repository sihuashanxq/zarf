using System.Data;

namespace Zarf.Core
{
    public interface IDbConnectionWrapper
    {
        IDbConnection DbConnection { get; }
        string ConnectionString { get; }
    }
}
