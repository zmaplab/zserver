using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZServer.Store;
using ZServer.Store.Configuration;

namespace ZServer;

public class ConfigurationSyncService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurationSyncService> _logger;

    public ConfigurationSyncService(IServiceProvider serviceProvider, ILogger<ConfigurationSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            var configurationProvider = _serviceProvider.GetRequiredService<ConfigurationProvider>();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshAsync(configurationProvider);
                }
                catch (Exception e)
                {
                    _logger.LogError("Load configuration store failed: {Exception}", e);
                }

                _logger.LogInformation("Load configuration store success");
                await Task.Delay(15000, cancellationToken);
            }
        }, cancellationToken);
    }

    public async Task RefreshAsync(ConfigurationProvider configurationProvider)
    {
        var configuration = configurationProvider.GetConfiguration();
        if (configuration != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var gridSetStore = scope.ServiceProvider.GetRequiredService<IGridSetStore>();
            await gridSetStore.Refresh(configuration);
            var sourceStore = scope.ServiceProvider.GetRequiredService<ISourceStore>();
            await sourceStore.Refresh(configuration);
            var styleGroupStore = scope.ServiceProvider.GetRequiredService<IStyleGroupStore>();
            await styleGroupStore.Refresh(configuration);
            var sldStore = scope.ServiceProvider.GetRequiredService<ISldStore>();
            await sldStore.Refresh(configuration);
            var resourceGroupStore = scope.ServiceProvider.GetRequiredService<IResourceGroupStore>();
            await resourceGroupStore.Refresh(configuration);
            var layerStore = scope.ServiceProvider.GetRequiredService<ILayerStore>();
            await layerStore.Refresh(configuration);
            var layerGroupStore = scope.ServiceProvider.GetRequiredService<ILayerGroupStore>();
            await layerGroupStore.Refresh(configuration);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}