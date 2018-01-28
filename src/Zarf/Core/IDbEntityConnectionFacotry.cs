namespace Zarf.Core
{
    public interface IDbEntityConnectionFacotry
    {
        IDbEntityConnection Create();

        IDbEntityConnection Create(string connectionString);

        IDbEntityConnection CreateDbContextScopedConnection();
    }
}
