using Microsoft.Extensions.DependencyInjection;

namespace Zarf.Core
{
    public interface IDbServiceBuilder
    {
        IDbService BuildService(string connectionString, IServiceCollection serviceCollection);

        IDbService BuildService(string connectionString);
    }
}
