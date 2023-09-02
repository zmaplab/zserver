using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ZMap;
using ZMap.Source;
using ZMap.Style;

namespace ZServer.Store
{
    public class LayerStore : ILayerStore
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> PropertyCache =
            new();

        private readonly IStyleGroupStore _styleStore;
        private readonly IResourceGroupStore _resourceGroupStore;
        private readonly ISourceStore _sourceStore;
        private readonly ILogger<LayerStore> _logger;
        private readonly ISldStore _sldStore;
        private static readonly ConcurrentDictionary<string, Layer> Cache = new();

        public LayerStore(IStyleGroupStore styleStore,
            IResourceGroupStore resourceGroupStore, ISourceStore sourceStore,
            ILogger<LayerStore> logger, ISldStore sldStore)
        {
            _styleStore = styleStore;
            _resourceGroupStore = resourceGroupStore;
            _sourceStore = sourceStore;
            _logger = logger;
            _sldStore = sldStore;
        }

        public async Task Refresh(IEnumerable<IConfiguration> configurations)
        {
            var existKeys = Cache.Keys.ToList();
            var keys = new List<string>();

            foreach (var configuration in configurations)
            {
                var sections = configuration.GetSection("layers");
                foreach (var section in sections.GetChildren())
                {
                    var resourceGroupName = section.GetValue<string>("resourceGroup");

                    var resourceGroup = string.IsNullOrWhiteSpace(resourceGroupName)
                        ? null
                        : await _resourceGroupStore.FindAsync(resourceGroupName);

                    var layer = await BindLayerAsync(section, resourceGroup);
                    if (layer == null)
                    {
                        continue;
                    }

                    keys.Add(layer.Name);
                    Cache.AddOrUpdate(layer.Name, layer, (_, _) => layer);
                }
            }

            var removedKeys = existKeys.Except(keys);
            foreach (var removedKey in removedKeys)
            {
                Cache.TryRemove(removedKey, out _);
            }
        }

        public async Task<Layer> FindAsync(string resourceGroupName, string layerName)
        {
            if (string.IsNullOrWhiteSpace(resourceGroupName) && string.IsNullOrWhiteSpace(layerName))
            {
                return null;
            }

            if (!Cache.TryGetValue(layerName, out var layer))
            {
                return null;
            }

            if (string.IsNullOrEmpty(resourceGroupName))
            {
                return await Task.FromResult(layer.Clone());
            }

            if (layer.ResourceGroup?.Name != resourceGroupName)
            {
                return null;
            }

            return await Task.FromResult(layer.Clone());

            // var result = await Cache.GetOrCreate($"{GetType().FullName}:{resourceGroupName}:{layerName}",
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
            //             _configuration.GetSection($"layers:{layerName}");
            //         var configResourceGroupName = section.GetValue<string>("resourceGroup");
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
            //         var layer = await BindLayerAsync(layerName, resourceGroup);
            //
            //         entry.SetValue(layer);
            //         entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
            //         return layer;
            //     });
            //
            // return result?.DeepClone();
        }

        public Task<List<Layer>> GetAllAsync()
        {
            var items = Cache.Values.Select(x => x.Clone()).ToList();
            return Task.FromResult(items);
        }

        public Task<List<Layer>> GetListAsync(string resourceGroup)
        {
            var result = new List<Layer>();
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

        private async Task<Layer> BindLayerAsync(IConfigurationSection section,
            ResourceGroup resourceGroup)
        {
            var services = section.GetSection("services").Get<HashSet<ServiceType>>();
            var sourceOrigin = await RestoreSourceAsync(resourceGroup?.Name, section.Key, section);
            if (sourceOrigin == null)
            {
                return null;
            }

            var source = sourceOrigin.Clone();
            var styleGroups = await RestoreStyleGroupsAsync(section);
            var extent = section.GetSection("extent").Get<double[]>();
            Envelope envelope = null;
            if (extent is { Length: 4 })
            {
                envelope = new Envelope(extent[0], extent[1], extent[2], extent[3]);
            }

            var layer = new Layer(resourceGroup, services, section.Key, source, styleGroups, envelope);
            section.Bind(layer);
            return layer;
        }

        private async Task<List<StyleGroup>> RestoreStyleGroupsAsync(IConfigurationSection section)
        {
            var group = new List<StyleGroup>();

            var styleNames = section.GetSection("styleGroups").Get<string[]>();

            if (styleNames.Length > 0)
            {
                foreach (var name in styleNames)
                {
                    var styleGroup = await _styleStore.FindAsync(name);
                    if (styleGroup != null)
                    {
                        group.Add(styleGroup.Clone());
                    }

                    var sldStyleGroups = await _sldStore.FindAsync(name);
                    if (sldStyleGroups != null)
                    {
                        group.AddRange(sldStyleGroups);
                    }
                }
            }

            return group;
        }

        private async Task<ISource> RestoreSourceAsync(string resourceGroup, string name, IConfigurationSection section)
        {
            var sourceName = section.GetValue<string>("source");
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                _logger.LogError("图层 {ResourceGroup}:{Name} 未配置数据源", resourceGroup, name);
                return null;
            }

            var source = await _sourceStore.FindAsync(sourceName);
            if (source == null)
            {
                _logger.LogError("图层 {ResourceGroup}:{Name} 的数据源 {SourceName} 不存在", resourceGroup, name, sourceName);
                return null;
            }

            var properties = PropertyCache.GetOrAdd(source.GetType(), t =>
            {
                return t.GetProperties().Where(z => z.CanWrite)
                    .ToList();
            });

            foreach (var child in section.GetChildren())
            {
                if (!child.Key.StartsWith("source") || child.Key == "source")
                {
                    continue;
                }

                var propertyName = child.Key.Replace("source", string.Empty);
                var property = properties
                    .FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                {
                    continue;
                }

                var value = child.Get(property.PropertyType);
                if (value != null)
                {
                    property.SetValue(source, value);
                }
            }

            return source;
        }
    }
}