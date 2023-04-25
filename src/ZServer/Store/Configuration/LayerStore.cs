using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Force.DeepCloner;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using ZMap;
using ZMap.Source;
using ZMap.Style;
using ZMap.Utilities;
using ZServer.Entity;

namespace ZServer.Store.Configuration
{
    public class LayerStore : ILayerStore
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache =
            new();

        private readonly IConfiguration _configuration;
        private readonly IStyleGroupStore _styleStore;
        private readonly IResourceGroupStore _resourceGroupStore;
        private readonly ISourceStore _sourceStore;
        private readonly ServerOptions _options;
        private readonly ILogger<LayerStore> _logger;
        private readonly ISldStore _sldStore;

        public LayerStore(IConfiguration configuration, IStyleGroupStore styleStore,
            IResourceGroupStore resourceGroupStore, ISourceStore sourceStore,
            IOptionsMonitor<ServerOptions> options, ILogger<LayerStore> logger, ISldStore sldStore)
        {
            _configuration = configuration;
            _styleStore = styleStore;
            _resourceGroupStore = resourceGroupStore;
            _sourceStore = sourceStore;
            _logger = logger;
            _sldStore = sldStore;
            _options = options.CurrentValue;
        }

        public async Task<ILayer> FindAsync(string resourceGroupName, string layerName)
        {
            if (string.IsNullOrWhiteSpace(resourceGroupName) || string.IsNullOrWhiteSpace(layerName))
            {
                return null;
            }

            var result = await Cache.GetOrCreate($"{GetType().FullName}:{resourceGroupName}:{layerName}",
                async entry =>
                {
                    var resourceGroup = await _resourceGroupStore.FindAsync(resourceGroupName);
                    if (resourceGroup == null)
                    {
                        _logger.LogError($"资源组 {resourceGroupName} 不存在");
                        return null;
                    }

                    var section =
                        _configuration.GetSection($"layers:{layerName}");
                    var configResourceGroupName = section.GetValue<string>("resourceGroup");

                    Layer layer = null;
                    if (configResourceGroupName == resourceGroupName)
                    {
                        layer = await BindLayerAsync(layerName, resourceGroup);
                    }

                    entry.SetValue(layer);
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                    return layer;
                });

            return result?.DeepClone();
        }

        public async Task<ILayer> FindAsync(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                return null;
            }

            var result = await Cache.GetOrCreate($"{GetType().FullName}:{layerName}",
                async entry =>
                {
                    var layer = await BindLayerAsync(layerName);
                    entry.SetValue(layer);
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                    return layer;
                });

            return result?.DeepClone();
        }

        private async Task<Layer> BindLayerAsync(string layerName, ResourceGroup resourceGroup)
        {
            var section =
                _configuration.GetSection($"layers:{layerName}");

            var services = section.GetSection("services").Get<HashSet<ServiceType>>();
            var source = await RestoreSourceAsync(resourceGroup.Name, layerName, section);
            var styleGroups = await RestoreStyleGroupsAsync(section);
            var extent = section.GetSection("extent").Get<double[]>();
            Envelope envelope = null;
            if (extent.Length == 4)
            {
                envelope = new Envelope(extent[0], extent[1], extent[2], extent[3]);
            }

            var layer = new LayerEntity(resourceGroup, services, layerName, source, styleGroups, envelope);
            section.Bind(layer);
            return layer;
        }

        private async Task<Layer> BindLayerAsync(string layerName)
        {
            var section =
                _configuration.GetSection($"layers:{layerName}");
            var resourceGroupName = section.GetSection("resourceGroup").Get<string>();
            var resourceGroup = await _resourceGroupStore.FindAsync(resourceGroupName);
            var services = section.GetSection("services").Get<HashSet<ServiceType>>();
            var source = await RestoreSourceAsync(resourceGroup?.Name, layerName, section);
            var styleGroups = await RestoreStyleGroupsAsync(section);
            var extent = section.GetSection("extent").Get<double[]>();
            Envelope envelope = null;
            if (extent.Length == 4)
            {
                envelope = new Envelope(extent[0], extent[1], extent[2], extent[3]);
            }

            var layer = new LayerEntity(resourceGroup, services, layerName, source, styleGroups, envelope);

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
                        group.Add(styleGroup);
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
                _logger.LogError($"图层 {resourceGroup}:{name} 未配置数据源");
                return null;
            }

            var source = await _sourceStore.FindAsync(sourceName);
            if (source == null)
            {
                _logger.LogError($"图层 {resourceGroup}:{name} 的数据源 {sourceName} 不存在");
                return null;
            }

            var properties = PropertyCache.GetOrAdd(source.GetType(), t =>
            {
                return t.GetProperties().Where(z => z.CanWrite)
                    .ToDictionary(y => y.Name, y => y);
            });

            foreach (var child in section.GetChildren())
            {
                if (!child.Key.StartsWith("source") || child.Key == "source")
                {
                    continue;
                }

                var propertyName = child.Key.Replace("source", string.Empty);
                if (properties.ContainsKey(propertyName))
                {
                    var property = properties[propertyName];
                    var value = child.Get(property.PropertyType);
                    if (value != null)
                    {
                        property.SetValue(source, value);
                    }
                }
            }

            return source;
        }

        public async Task<List<ILayer>> GetAllAsync()
        {
            var result = new List<ILayer>();
            foreach (var child in _configuration.GetSection("layers").GetChildren())
            {
                result.Add(await FindAsync(child.Key));
            }

            return result;
        }

        public async Task<List<ILayer>> GetListAsync(string resourceGroup)
        {
            var result = new List<ILayer>();
            if (string.IsNullOrWhiteSpace(resourceGroup))
            {
                return result;
            }

            foreach (var child in _configuration.GetSection("layers").GetChildren())
            {
                var layer = await FindAsync(resourceGroup, child.Key);
                if (layer != null)
                {
                    result.Add(layer);
                }
            }

            return result;
        }
    }
}