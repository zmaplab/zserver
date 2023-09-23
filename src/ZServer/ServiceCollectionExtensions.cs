using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZMap.Ogc.Wms;
using ZMap.Ogc.Wmts;
using ZMap.Renderer.SkiaSharp;
using ZMap.Store;
using ZServer.Store;
using ConfigurationProvider = ZServer.Store.ConfigurationProvider;

[assembly: InternalsVisibleTo("ZServer.Tests")]

namespace ZServer
{
    public static class ServiceCollectionExtensions
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static ZServerBuilder AddZServer(this IServiceCollection serviceCollection,
            IConfiguration configuration, string layersConfiguration)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            serviceCollection.Configure<ServerOptions>(configuration);

            // 配置的存储
            serviceCollection.TryAddScoped<ILayerStore, LayerStore>();
            serviceCollection.TryAddScoped<IResourceGroupStore, ResourceGroupStore>();
            serviceCollection.TryAddScoped<ISourceStore, SourceStore>();
            serviceCollection.TryAddScoped<IGridSetStore, GridSetStore>();
            serviceCollection.TryAddScoped<IStyleGroupStore, StyleGroupStore>();
            serviceCollection.TryAddScoped<ILayerGroupStore, LayerGroupStore>();
            serviceCollection.TryAddScoped<ISldStore, SldStore>();
            serviceCollection.TryAddSingleton(_ => new ConfigurationProvider(layersConfiguration));
            serviceCollection.AddHostedService<ConfigurationStoreRefreshService>();

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
}