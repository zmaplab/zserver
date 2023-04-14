using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Force.DeepCloner;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZMap;
using ZMap.Infrastructure;
using ZServer.Entity;

namespace ZServer.Store.Configuration
{
    public class LayerGroupStore : ILayerGroupStore
    {
        private readonly IConfiguration _configuration;
        private readonly IResourceGroupStore _resourceGroupStore;
        private readonly ILayerStore _layerStore;
        private readonly ILogger<LayerGroupStore> _logger;
        private readonly ServerOptions _options;

        public LayerGroupStore(IConfiguration configuration,
            IResourceGroupStore resourceGroupStore, ILayerStore layerStore,
            ILogger<LayerGroupStore> logger, IOptionsMonitor<ServerOptions> options)
        {
            _configuration = configuration;
            _resourceGroupStore = resourceGroupStore;
            _layerStore = layerStore;
            _logger = logger;
            _options = options.CurrentValue;
        }

        public async Task<LayerGroup> FindAsync(string resourceGroupName, string layerGroupName)
        {
            if (string.IsNullOrWhiteSpace(resourceGroupName) || string.IsNullOrWhiteSpace(layerGroupName))
            {
                return null;
            }

            var result = await Cache.GetOrCreate($"{GetType().FullName}:{resourceGroupName}:{layerGroupName}",
                async entry =>
                {
                    var resourceGroup = await _resourceGroupStore.FindAsync(resourceGroupName);
                    if (resourceGroup == null)
                    {
                        return null;
                    }

                    var section =
                        _configuration.GetSection($"layerGroups:{layerGroupName}");

                    LayerGroup layerGroup = null;
                    var configResourceGroupName = section.GetSection("resourceGroup").Get<string>();
                    if (configResourceGroupName == resourceGroupName)
                    {
                        layerGroup = Activator.CreateInstance<LayerGroup>();
                        layerGroup.Services = section.GetSection("ogcWebServices").Get<HashSet<ServiceType>>();
                        layerGroup.Name = layerGroupName;
                        layerGroup.ResourceGroup = resourceGroup;
                        layerGroup.Layers = new List<ILayer>();

                        await RestoreAsync(layerGroup, section);

                        if (layerGroup.Layers.Count == 0)
                        {
                            _logger.LogWarning($"图层组 {layerGroupName} 中的没有有效图层");
                        }
                    }

                    entry.SetValue(layerGroup);
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                    return layerGroup;
                });

            if (result == null)
            {
                return null;
            }
            var layerGroup= result.DeepClone();
            foreach (var layer in layerGroup.Layers)
            {
                layer.ClearEnvironments();
            }

            return layerGroup;
        }

        public async Task<LayerGroup> FindAsync(string layerGroupName)
        {
            return string.IsNullOrWhiteSpace(layerGroupName)
                ? null
                : await Cache.GetOrCreate($"{GetType().FullName}:{layerGroupName}",
                    async entry =>
                    {
                        var section =
                            _configuration.GetSection($"layerGroups:{layerGroupName}");
                        LayerGroup layerGroup = null;
                        if (section.GetChildren().Any())
                        {
                            layerGroup = Activator.CreateInstance<LayerGroup>();
                            layerGroup.Services =
                                section.GetSection("ogcWebServices").Get<HashSet<ServiceType>>();
                            layerGroup.Name = layerGroupName;

                            var resourceGroupName = section.GetSection("resourceGroup").Get<string>();
                            layerGroup.ResourceGroup = await _resourceGroupStore.FindAsync(resourceGroupName);
                            layerGroup.Layers = new List<ILayer>();

                            await RestoreAsync(layerGroup, section);

                            if (layerGroup.Layers.Count == 0)
                            {
                                _logger.LogWarning($"图层组 {layerGroupName} 中的没有有效图层");
                            }
                        }

                        entry.SetValue(layerGroup);
                        entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                        return layerGroup;
                    });
        }

        public async Task<List<LayerGroup>> GetAllAsync()
        {
            var result = new List<LayerGroup>();
            foreach (var child in _configuration.GetSection("layerGroups").GetChildren())
            {
                result.Add(await FindAsync(child.Key));
            }

            return result;
        }

        public async Task<List<LayerGroup>> GetListAsync(string resourceGroup)
        {
            var result = new List<LayerGroup>();
            if (string.IsNullOrWhiteSpace(resourceGroup))
            {
                return result;
            }

            foreach (var child in _configuration.GetSection("layerGroups").GetChildren())
            {
                var layer = await FindAsync(resourceGroup, child.Key);
                if (layer != null)
                {
                    result.Add(layer);
                }
            }

            return result;
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
                            layer = await _layerStore.FindAsync(layerName);
                            break;
                        case 2:
                            layer = await _layerStore.FindAsync(parts[0], parts[1]);
                            break;
                        default:
                            _logger.LogError($"图层组 {layerGroup.Name} 中的图层 {layerName} 不存在");
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