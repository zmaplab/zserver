using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZMap;
using ZServer.Entity;

namespace ZServer.Store.Configuration
{
    public class LayerGroupStore : ILayerGroupStore
    {
        private readonly IResourceGroupStore _resourceGroupStore;
        private readonly ILayerStore _layerStore;
        private readonly ILogger<LayerGroupStore> _logger;
        private static readonly ConcurrentDictionary<string, LayerGroup> Cache = new();

        public LayerGroupStore(
            IResourceGroupStore resourceGroupStore, ILayerStore layerStore,
            ILogger<LayerGroupStore> logger)
        {
            _resourceGroupStore = resourceGroupStore;
            _layerStore = layerStore;
            _logger = logger;
        }

        public async Task Refresh(IConfiguration configuration)
        {
            var sections = configuration.GetSection("layerGroups");
            foreach (var section in sections.GetChildren())
            {
                var resourceGroupName = section.GetValue<string>("resourceGroup");

                var resourceGroup = string.IsNullOrWhiteSpace(resourceGroupName)
                    ? null
                    : await _resourceGroupStore.FindAsync(resourceGroupName);

                var layerGroup = Activator.CreateInstance<LayerGroup>();
                layerGroup.Services = section.GetSection("ogcWebServices").Get<HashSet<ServiceType>>();
                layerGroup.Name = section.Key;
                layerGroup.ResourceGroup = resourceGroup;
                layerGroup.Layers = new List<ILayer>();
                await RestoreAsync(layerGroup, section);
                Cache.AddOrUpdate(section.Key, layerGroup, (_, _) => layerGroup);
            }
        }

        public async Task<LayerGroup> FindAsync(string resourceGroupName, string layerGroupName)
        {
            if (string.IsNullOrWhiteSpace(resourceGroupName) && string.IsNullOrWhiteSpace(layerGroupName))
            {
                return null;
            }

            if (!Cache.TryGetValue(layerGroupName, out var layerGroup))
            {
                return null;
            }

            if (string.IsNullOrEmpty(resourceGroupName))
            {
                return await Task.FromResult(layerGroup.Clone());
            }

            if (layerGroup.ResourceGroup?.Name != resourceGroupName)
            {
                return null;
            }

            return await Task.FromResult(layerGroup.Clone());

            // var item = await Cache.GetOrCreate($"{GetType().FullName}:{resourceGroupName}:{layerGroupName}",
            //     async entry =>
            //     {
            //         ResourceGroup resourceGroup = null;
            //         // 若传的资源组不为空，才需要查询资源组信息
            //         if (!string.IsNullOrEmpty(resourceGroupName))
            //         {
            //             // 若资源组不存在，则返回空
            //             resourceGroup = await _resourceGroupStore.FindAsync(resourceGroupName);
            //             if (resourceGroup == null)
            //             {
            //                 _logger.LogError("资源组 {ResourceGroupName} 不存在", resourceGroupName);
            //                 return null;
            //             }
            //         }
            //
            //         var section =
            //             _configuration.GetSection($"layerGroups:{layerGroupName}");
            //
            //         var configResourceGroupName = section.GetSection("resourceGroup").Get<string>();
            //
            //         // 若传的资源组不为空，并且代码执行到此处，说明资源组存在
            //         if (!string.IsNullOrEmpty(resourceGroupName))
            //         {
            //             // 若配置的资源组与查询资源组不一致，则返回空
            //             if (configResourceGroupName != resourceGroupName)
            //             {
            //                 return null;
            //             }
            //         }
            //
            //         if (resourceGroup == null && !string.IsNullOrEmpty(configResourceGroupName))
            //         {
            //             resourceGroup = await _resourceGroupStore.FindAsync(configResourceGroupName);
            //         }
            //
            //         var layerGroup = Activator.CreateInstance<LayerGroup>();
            //         layerGroup.Services = section.GetSection("ogcWebServices").Get<HashSet<ServiceType>>();
            //         layerGroup.Name = layerGroupName;
            //         layerGroup.ResourceGroup = resourceGroup;
            //         layerGroup.Layers = new List<ILayer>();
            //
            //         await RestoreAsync(layerGroup, section);
            //
            //         if (layerGroup.Layers.Count == 0)
            //         {
            //             _logger.LogWarning("图层组 {LayerGroupName} 中的没有有效图层", layerGroupName);
            //         }
            //
            //         entry.SetValue(layerGroup);
            //         entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
            //         return layerGroup;
            //     });
            // // 1. 从 configuration 中解析对象非常耗时，因此需要使用缓存
            // // 2. 但是若是直接返回对象，会导致对象被复用（修改），状态可能不正确
            // return item?.Clone();
        }

        public Task<List<LayerGroup>> GetAllAsync()
        {
            var items = Cache.Values.Select(x => x.Clone()).ToList();
            return Task.FromResult(items);
        }

        public Task<List<LayerGroup>> GetListAsync(string resourceGroup)
        {
            var result = new List<LayerGroup>();
            if (string.IsNullOrWhiteSpace(resourceGroup))
            {
                return Task.FromResult(result);
            }

            foreach (var value in Cache.Values)
            {
                if (value.ResourceGroup?.Name == resourceGroup)
                {
                    result.Add(value.Clone());
                }
            }

            return Task.FromResult(result);
        }


        private async Task RestoreAsync(LayerGroup layerGroup, IConfigurationSection section)
        {
            var layerNames = section.GetSection("layers").Get<HashSet<string>>();
            if (layerNames != null)
            {
                foreach (var layerName in layerNames)
                {
                    var parts = layerName.Split(':');
                    ILayer layer = null;
                    switch (parts.Length)
                    {
                        case 1:
                            layer = await _layerStore.FindAsync(null, layerName);
                            break;
                        case 2:
                            layer = await _layerStore.FindAsync(parts[0], parts[1]);
                            break;
                        default:
                            _logger.LogError("图层组 {LayerGroupName} 中的图层 {LayerName} 不存在", layerGroup.Name, layerName);
                            break;
                    }

                    if (layer != null)
                    {
                        layerGroup.Layers.Add(layer);
                    }
                }
            }
        }
    }
}