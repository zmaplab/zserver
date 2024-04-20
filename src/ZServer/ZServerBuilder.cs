using Microsoft.Extensions.DependencyInjection;

namespace ZServer;

public class ZServerBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}