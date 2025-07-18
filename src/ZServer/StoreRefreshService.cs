using System;
using System.Collections.Generic;
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
            var configurationProvider = serviceProvider.GetRequiredService<IJsonStoreProvider>();
            configurationProvider.Check();

            var logged = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshAsync(configurationProvider, logger);
                    if (!logged)
                    {
                        logger.LogInformation("刷新配置完成");
                        logged = true;
                    }
                    else
                    {
                        logger.LogDebug("刷新配置完成");
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "加载配置失败");
                }

                await Task.Delay(10000, cancellationToken);
            }
        }, cancellationToken);
    }

    public async Task RefreshAsync(IJsonStoreProvider jsonStoreProvider, ILogger logger)
    {
        var configuration = await jsonStoreProvider.GetConfigurationAsync();
        if (configuration != null)
        {
            var configurations = new List<JObject> { configuration };
            using var scope = serviceProvider.CreateScope();
            var gridSetStore = scope.ServiceProvider.GetRequiredService<IGridSetStore>();
            await gridSetStore.RefreshAsync(configurations);
            var sourceStore = scope.ServiceProvider.GetRequiredService<ISourceStore>();
            await sourceStore.RefreshAsync(configurations);
            try
            {
                var styleGroupStore = scope.ServiceProvider.GetRequiredService<IStyleGroupStore>();
                await styleGroupStore.RefreshAsync(configurations);
            }
            catch (Exception e)
            {
                logger.LogError("加载样式失败: {Exception}", e);
            }

            try
            {
                var sldStore = scope.ServiceProvider.GetRequiredService<ISldStore>();
                await sldStore.RefreshAsync(configurations);
            }
            catch (Exception e)
            {
                logger.LogError("加载 SLD 样式失败: {Exception}", e);
            }

            var resourceGroupStore = scope.ServiceProvider.GetRequiredService<IResourceGroupStore>();
            await resourceGroupStore.RefreshAsync(configurations);
            var layerStore = scope.ServiceProvider.GetRequiredService<ILayerStore>();
            await layerStore.RefreshAsync(configurations);
            var layerGroupStore = scope.ServiceProvider.GetRequiredService<ILayerGroupStore>();
            await layerGroupStore.RefreshAsync(configurations);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}