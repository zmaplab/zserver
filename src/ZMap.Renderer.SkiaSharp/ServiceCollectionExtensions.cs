using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ZMap.Renderer.SkiaSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSkiaSharp(this IServiceCollection services)
    {
        services.TryAddSingleton<IGraphicsServiceProvider, GraphicsServiceProvider>();

        return services;
    }
}