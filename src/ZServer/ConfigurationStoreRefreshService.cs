using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZMap.Store;
using ZServer.Store;
using ConfigurationProvider = ZServer.Store.ConfigurationProvider;

namespace ZServer;

public class ConfigurationStoreRefreshService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurationStoreRefreshService> _logger;

    public ConfigurationStoreRefreshService(IServiceProvider serviceProvider,
        ILogger<ConfigurationStoreRefreshService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            var configurationProvider = _serviceProvider.GetRequiredService<ConfigurationProvider>();
            if (File.Exists(configurationProvider.Path))
            {
                _logger.LogInformation("ZServer 已发现配置文件 {ConfigurationPath} ",
                    configurationProvider.Path);
            }
            else
            {
                _logger.LogError("ZServer 未发现配置文件 {ConfigurationPath}", configurationProvider.Path);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshAsync(configurationProvider);
                }
                catch (Exception e)
                {
                    _logger.LogError("加载配置文件存储失败: {Exception}", e);
                }

                await Task.Delay(15000, cancellationToken);
            }
        }, cancellationToken);
    }

    public async Task RefreshAsync(ConfigurationProvider configurationProvider)
    {
        var configuration = configurationProvider.GetConfiguration();
        if (configuration != null)
        {
            var configurations = new List<IConfiguration> { configuration };
            using var scope = _serviceProvider.CreateScope();
            var gridSetStore = scope.ServiceProvider.GetRequiredService<IGridSetStore>();
            await gridSetStore.Refresh(configurations);
            var sourceStore = scope.ServiceProvider.GetRequiredService<ISourceStore>();
            await sourceStore.Refresh(configurations);
            var styleGroupStore = scope.ServiceProvider.GetRequiredService<IStyleGroupStore>();
            await styleGroupStore.Refresh(configurations);
            var sldStore = scope.ServiceProvider.GetRequiredService<ISldStore>();
            await sldStore.Refresh(configurations);
            var resourceGroupStore = scope.ServiceProvider.GetRequiredService<IResourceGroupStore>();
            await resourceGroupStore.Refresh(configurations);
            var layerStore = scope.ServiceProvider.GetRequiredService<ILayerStore>();
            await layerStore.Refresh(configurations);
            var layerGroupStore = scope.ServiceProvider.GetRequiredService<ILayerGroupStore>();
            await layerGroupStore.Refresh(configurations);

            _logger.LogInformation("刷新配置文件存储成功");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}