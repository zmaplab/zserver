using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZServer.HashAlgorithm;
using ZServer.Store;
using ZServer.Store.Configuration;

[assembly: InternalsVisibleTo("ZServer.Tests")]

namespace ZServer
{
    public static class ServiceCollectionExtensions
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static ZServerBuilder AddZServer(this IServiceCollection serviceCollection,
            IConfiguration configuration)
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

            serviceCollection.TryAddSingleton<IHashAlgorithmService, MurmurHashAlgorithmService>();

            serviceCollection.AddHostedService<PreloadService>();
            serviceCollection.TryAddScoped<ILayerQuerier, LayerQuerier>();
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