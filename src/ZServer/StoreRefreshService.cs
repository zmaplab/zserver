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

namespace ZServer;

public class StoreRefreshService(
    IServiceProvider serviceProvider)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<StoreRefreshService>>();
            var configurationProvider = serviceProvider.GetRequiredService<JsonStoreProvider>();
            if (File.Exists(configurationProvider.Path))
            {
                logger.LogInformation("ZServer 发现配置文件 {ConfigurationPath} ",
                    configurationProvider.Path);
            }
            else
            {
                logger.LogError("ZServer 未发现配置文件 {ConfigurationPath}", configurationProvider.Path);
            }

            var logged = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshAsync(configurationProvider);
                    if (!logged)
                    {
                        logger.LogInformation("刷新配置文件成功");
                        logged = true;
                    }
                    else
                    {
                        logger.LogDebug("刷新配置文件成功");
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "加载配置文件失败");
                }

                await Task.Delay(15000, cancellationToken);
            }
        }, cancellationToken);
    }

    public async Task RefreshAsync(JsonStoreProvider jsonStoreProvider)
    {
        var configuration = jsonStoreProvider.GetConfiguration();
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
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}