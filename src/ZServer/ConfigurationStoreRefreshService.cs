using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ZMap.Store;
using ZServer.Store;
using ConfigurationProvider = ZServer.Store.ConfigurationProvider;

namespace ZServer;

public class ConfigurationStoreRefreshService(
    IServiceProvider serviceProvider,
    ILogger<ConfigurationStoreRefreshService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            var configurationProvider = serviceProvider.GetRequiredService<ConfigurationProvider>();
            if (File.Exists(configurationProvider.Path))
            {
                logger.LogInformation("ZServer 发现配置文件 {ConfigurationPath} ",
                    configurationProvider.Path);
            }
            else
            {
                logger.LogError("ZServer 未发现配置文件 {ConfigurationPath}", configurationProvider.Path);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshAsync(configurationProvider);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "加载配置文件失败");
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
            var configurations = new List<JObject> { configuration };
            using var scope = serviceProvider.CreateScope();
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

            logger.LogInformation("刷新配置文件成功");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}