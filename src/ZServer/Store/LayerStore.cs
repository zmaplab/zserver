using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using ZMap;
using ZMap.Infrastructure;
using ZMap.Source;
using ZMap.Style;

namespace ZServer.Store;

public class LayerStore(
    IStyleGroupStore styleStore,
    IResourceGroupStore resourceGroupStore,
    ISourceStore sourceStore,
    ISldStore sldStore)
    : ILayerStore
{
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> PropertyCache =
        new();

    private static readonly ILogger Logger = Log.CreateLogger<LayerStore>();
    private static readonly ConcurrentDictionary<string, Layer> Cache = new();

    public async Task RefreshAsync(List<JObject> configurations)
    {
        var existKeys = Cache.Keys.ToList();
        var keys = new List<string>();

        foreach (var configuration in configurations)
        {
            var sections = configuration.SelectToken("layers");
            if (sections == null)
            {
                continue;
            }

            foreach (var section in sections.Children<JProperty>())
            {
                var name = section.Name;
                var resourceGroupName = section.Value["resourceGroup"]?.ToObject<string>();

                var resourceGroup = string.IsNullOrWhiteSpace(resourceGroupName)
                    ? null
                    : await resourceGroupStore.FindAsync(resourceGroupName);

                var layer = await BindLayerAsync(name, section.Value as JObject, resourceGroup);
                if (layer == null)
                {
                    continue;
                }

                keys.Add(name);
                Cache.AddOrUpdate(name, layer, (_, _) => layer);
            }
        }

        var removedKeys = existKeys.Except(keys);
        foreach (var removedKey in removedKeys)
        {
            Cache.TryRemove(removedKey, out _);
        }
    }

    // public async Task Refresh(IEnumerable<IConfiguration> configurations)
    // {
    //     var existKeys = Cache.Keys.ToList();
    //     var keys = new List<string>();
    //
    //     foreach (var configuration in configurations)
    //     {
    //         var sections = configuration.GetSection("layers");
    //         foreach (var section in sections.GetChildren())
    //         {
    //             var resourceGroupName = section.GetValue<string>("resourceGroup");
    //
    //             var resourceGroup = string.IsNullOrWhiteSpace(resourceGroupName)
    //                 ? null
    //                 : await resourceGroupStore.FindAsync(resourceGroupName);
    //
    //             var layer = await BindLayerAsync(section, resourceGroup);
    //             if (layer == null)
    //             {
    //                 continue;
    //             }
    //
    //             keys.Add(layer.Name);
    //             Cache.AddOrUpdate(layer.Name, layer, (_, _) => layer);
    //         }
    //     }
    //
    //     var removedKeys = existKeys.Except(keys);
    //     foreach (var removedKey in removedKeys)
    //     {
    //         Cache.TryRemove(removedKey, out _);
    //     }
    // }

    public ValueTask<Layer> FindAsync(string layerName)
    {
        return Cache.TryGetValue(layerName, out var item)
            ? new ValueTask<Layer>(item.Clone())
            : new ValueTask<Layer>();
    }

    public async ValueTask<Layer> FindAsync(string resourceGroupName, string layerName)
    {
        if (string.IsNullOrEmpty(resourceGroupName))
        {
            return null;
        }

        var layer = await FindAsync(layerName);
        if (layer == null || layer.ResourceGroup?.Name != resourceGroupName)
        {
            return null;
        }

        return layer;
    }

    public ValueTask<List<Layer>> GetAllAsync()
    {
        var items = Cache.Values.Select(x => x.Clone()).ToList();
        return new ValueTask<List<Layer>>(items);
    }

    public ValueTask<List<Layer>> GetListAsync(string resourceGroup)
    {
        var result = new List<Layer>();
        if (string.IsNullOrEmpty(resourceGroup))
        {
            return new ValueTask<List<Layer>>(result);
        }

        foreach (var value in Cache.Values)
        {
            if (value.ResourceGroup?.Name == resourceGroup)
            {
                result.Add(value.Clone());
            }
        }

        return new ValueTask<List<Layer>>(result);
    }

    // private async Task<Layer> BindLayerAsync(IConfigurationSection section,
    //     ResourceGroup resourceGroup)
    // {
    //     var services = section.GetSection("services").Get<HashSet<ServiceType>>();
    //     var sourceOrigin = await RestoreSourceAsync(resourceGroup?.Name, section.Key, section);
    //     if (sourceOrigin == null)
    //     {
    //         return null;
    //     }
    //
    //     var source = sourceOrigin.Clone();
    //     var styleGroups = await RestoreStyleGroupsAsync(section);
    //     var extent = section.GetSection("extent").Get<double[]>();
    //     Envelope envelope = null;
    //     if (extent is { Length: 4 })
    //     {
    //         envelope = new Envelope(extent[0], extent[1], extent[2], extent[3]);
    //     }
    //
    //     var layer = new Layer(resourceGroup, services, section.Key, source, styleGroups, envelope);
    //     section.Bind(layer);
    //     return layer;
    // }

    private async Task<Layer> BindLayerAsync(string name, JObject section,
        ResourceGroup resourceGroup)
    {
        if (section == null)
        {
            return null;
        }

        var servicesToken = section["services"];
        var services = servicesToken == null
            ? new HashSet<ServiceType>()
            : servicesToken.ToObject<HashSet<ServiceType>>();
        var sourceOrigin = await RestoreSourceAsync(resourceGroup?.Name, name, section);
        if (sourceOrigin == null)
        {
            return null;
        }

        var source = sourceOrigin.Clone();
        var styleGroups = await RestoreStyleGroupsAsync(section);
        var extent = section["extent"]?.ToObject<double[]>();
        Envelope envelope = null;
        if (extent is { Length: 4 })
        {
            envelope = new Envelope(extent[0], extent[1], extent[2], extent[3]);
        }

        var layer = new Layer(resourceGroup, services, name, source, styleGroups, envelope)
        {
            Buffers = section["buffers"]?.ToObject<List<GridBuffer>>() ?? new List<GridBuffer>(),
            Enabled = section["enabled"]?.ToObject<bool>() ?? true
        };
        return layer;
    }

    // private async Task<List<StyleGroup>> RestoreStyleGroupsAsync(IConfigurationSection section)
    // {
    //     var group = new List<StyleGroup>();
    //
    //     var styleNames = section.GetSection("styleGroups").Get<string[]>();
    //
    //     if (styleNames is { Length: > 0 })
    //     {
    //         foreach (var name in styleNames)
    //         {
    //             var styleGroup = await styleStore.FindAsync(name);
    //             if (styleGroup != null)
    //             {
    //                 group.Add(styleGroup.Clone());
    //             }
    //
    //             var sldStyleGroups = await sldStore.FindAsync(name);
    //             if (sldStyleGroups != null)
    //             {
    //                 group.AddRange(sldStyleGroups);
    //             }
    //         }
    //     }
    //
    //     return group;
    // }

    private async Task<List<StyleGroup>> RestoreStyleGroupsAsync(JObject section)
    {
        var group = new List<StyleGroup>();

        var styleNames = section["styleGroups"]?.ToObject<string[]>();

        if (styleNames is not { Length: > 0 })
        {
            return group;
        }

        foreach (var name in styleNames)
        {
            var styleGroup = await styleStore.FindAsync(name);
            if (styleGroup != null)
            {
                group.Add(styleGroup.Clone());
            }

            var sldStyleGroups = await sldStore.FindAsync(name);
            if (sldStyleGroups != null)
            {
                group.AddRange(sldStyleGroups.Select(x => x.Clone()));
            }
        }

        return group;
    }

    // private async Task<ISource> RestoreSourceAsync(string resourceGroup, string name, IConfigurationSection section)
    // {
    //     var sourceName = section.GetValue<string>("source");
    //     if (string.IsNullOrWhiteSpace(sourceName))
    //     {
    //         Logger.LogError("图层 {ResourceGroup}:{Name} 未配置数据源", resourceGroup, name);
    //         return null;
    //     }
    //
    //     var source = await sourceStore.FindAsync(sourceName);
    //     if (source == null)
    //     {
    //         Logger.LogError("图层 {ResourceGroup}:{Name} 的数据源 {SourceName} 不存在", resourceGroup, name, sourceName);
    //         return null;
    //     }
    //
    //     var properties = PropertyCache.GetOrAdd(source.GetType(), t =>
    //     {
    //         return t.GetProperties().Where(z => z.CanWrite)
    //             .ToList();
    //     });
    //
    //     foreach (var child in section.GetChildren())
    //     {
    //         if (!child.Key.StartsWith("source") || child.Key == "source")
    //         {
    //             continue;
    //         }
    //
    //         var propertyName = child.Key.Replace("source", string.Empty);
    //         var property = properties
    //             .FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    //         if (property == null)
    //         {
    //             continue;
    //         }
    //
    //         var value = child.Get(property.PropertyType);
    //         if (value != null)
    //         {
    //             property.SetValue(source, value);
    //         }
    //     }
    //
    //     return source;
    // }

    private async Task<ISource> RestoreSourceAsync(string resourceGroup, string name, JObject section)
    {
        var sourceName = section["source"]?.ToObject<string>();
        if (string.IsNullOrEmpty(sourceName))
        {
            Logger.LogError("图层 {ResourceGroup}:{Name} 未配置数据源", resourceGroup, name);
            return null;
        }

        var source = await sourceStore.FindAsync(sourceName);
        if (source == null)
        {
            Logger.LogError("图层 {ResourceGroup}:{Name} 的数据源 {SourceName} 不存在", resourceGroup, name, sourceName);
            return null;
        }

        source.Key = name;
        var properties = PropertyCache.GetOrAdd(source.GetType(), t =>
        {
            return t.GetProperties().Where(z => z.CanWrite)
                .ToList();
        });

        foreach (var child in section.Children<JProperty>())
        {
            if (!child.Name.StartsWith("source") || child.Name == "source")
            {
                continue;
            }

            var propertyName = child.Name.Replace("source", string.Empty);
            var property = properties
                .FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
            {
                continue;
            }

            var value = child.ToObject(property.PropertyType);
            if (value != null)
            {
                property.SetValue(source, value);
            }
        }

        return source;
    }
}