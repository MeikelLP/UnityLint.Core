using Microsoft.Extensions.DependencyInjection;

namespace Editor
{
    public interface IServiceConfiguration
    {
        void ConfigureServices(ServiceCollection services);
    }
}