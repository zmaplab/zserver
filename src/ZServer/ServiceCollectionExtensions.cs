using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ZMap;
using ZMap.Ogc.Wms;
using ZMap.Ogc.Wmts;
using ZMap.Renderer.SkiaSharp;
using ZMap.Store;
using ZServer.Store;

[assembly: InternalsVisibleTo("ZServer.Tests")]

namespace ZServer;

public static class ServiceCollectionExtensions
{
    public static ZServerBuilder AddZServer(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        serviceCollection.Configure<ServerOptions>(configuration);
        var configSection = configuration.GetSection("config");
        var configOptions = configSection.Get<StoreConfigOptions>();
        if (configOptions == null)
        {
            serviceCollection.AddSingleton<IJsonStoreProvider>(provider =>
                new FileJsonStoreProvider("conf/zserver.json",
                    provider.GetRequiredService<ILogger<FileJsonStoreProvider>>()));
        }
        else
        {
            serviceCollection.Configure<StoreConfigOptions>(configSection);
            var configProvider = "socodb".Equals(configOptions.Provider, StringComparison.OrdinalIgnoreCase)
                ? "socodb"
                : "file";

            switch (configProvider.ToLower())
            {
                case "file":
                    var configAddr = string.IsNullOrEmpty(configOptions.Address)
                        ? "conf/zserver.json"
                        : configOptions.Address;
                    serviceCollection.AddSingleton<IJsonStoreProvider>(provider =>
                        new FileJsonStoreProvider(configAddr,
                            provider.GetRequiredService<ILogger<FileJsonStoreProvider>>()));
                    break;
                case "socodb":
                    if (string.IsNullOrEmpty(configOptions.Address))
                    {
                        throw new ArgumentNullException(nameof(configOptions.Address));
                    }

                    serviceCollection.AddSingleton<IJsonStoreProvider>(provider =>
                        new SocoStoreProvider(configOptions.Address, configOptions,
                            provider.GetRequiredService<IHttpClientFactory>(),
                            provider.GetRequiredService<ILogger<SocoStoreProvider>>()));
                    break;
                default:
                    throw new NotSupportedException($"不支持的配置提供者 {configProvider}");
            }
        }

        // 配置的存储
        serviceCollection.TryAddScoped<ILayerStore, LayerStore>();
        serviceCollection.TryAddScoped<IResourceGroupStore, ResourceGroupStore>();
        serviceCollection.TryAddScoped<ISourceStore, SourceStore>();
        serviceCollection.TryAddScoped<IGridSetStore, GridSetStore>();
        serviceCollection.TryAddScoped<IStyleGroupStore, StyleGroupStore>();
        serviceCollection.TryAddScoped<ILayerGroupStore, LayerGroupStore>();
        serviceCollection.TryAddScoped<ISldStore, SldStore>();
        serviceCollection.AddHostedService<RefreshConfigService>();
        serviceCollection.AddHostedService<PreloadService>();
        serviceCollection.TryAddScoped<ILayerQueryService, LayerQueryService>();
        serviceCollection.TryAddScoped<WmsService>();
        serviceCollection.TryAddScoped<WmtsService>();
        serviceCollection.AddMemoryCache();
        return new ZServerBuilder(serviceCollection);
    }

    public static ZServerBuilder AddSkiaSharpRenderer(this ZServerBuilder serverBuilder)
    {
        serverBuilder.Services.AddSkiaSharp();
        return serverBuilder;
    }
}