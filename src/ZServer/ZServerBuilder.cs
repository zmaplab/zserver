using Microsoft.Extensions.DependencyInjection;

namespace ZServer;

public class ZServerBuilder
{
    public ZServerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}