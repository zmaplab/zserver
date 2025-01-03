using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ZMap;
using ZMap.Infrastructure;

namespace ZServer.Store;

public class LayerGroupStore(
    IResourceGroupStore resourceGroupStore,
    ILayerStore layerStore)
    : ILayerGroupStore
{
    private static Dictionary<string, LayerGroup> _cache = new();
    private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<LayerGroupStore>());

    public async Task RefreshAsync(List<JObject> configurations)
    {
        var dict = new Dictionary<string, LayerGroup>();

        foreach (var configuration in configurations)
        {
            var sections = configuration.SelectToken("layerGroups");
            if (sections == null)
            {
                continue;
            }

            foreach (var section in sections.Children<JProperty>())
            {
                var name = section.Name;
                var obj = section.Value as JObject;
                if (obj == null)
                {
                    continue;
                }

                var resourceGroupName = obj["resourceGroup"]?.ToObject<string>();

                var resourceGroup = string.IsNullOrWhiteSpace(resourceGroupName)
                    ? null
                    : await resourceGroupStore.FindAsync(resourceGroupName);
                var servicesToken = obj["services"];
                var services = servicesToken == null
                    ? new HashSet<ServiceType>()
                    : servicesToken.ToObject<HashSet<ServiceType>>();

                var layerGroup = Activator.CreateInstance<LayerGroup>();
                layerGroup.Services = services;
                layerGroup.Name = name;
                layerGroup.ResourceGroup = resourceGroup;
                layerGroup.Layers = new List<Layer>();
                await RestoreAsync(layerGroup, obj);
                dict.Add(name, layerGroup);
            }
        }

        _cache = dict;
    }

    // public async Task Refresh(IEnumerable<IConfiguration> configurations)
    // {
    //     var existKeys = Cache.Keys.ToList();
    //     var keys = new List<string>();
    //
    //     foreach (var configuration in configurations)
    //     {
    //         var sections = configuration.GetSection("layerGroups");
    //         foreach (var section in sections.GetChildren())
    //         {
    //             var resourceGroupName = section.GetValue<string>("resourceGroup");
    //
    //             var resourceGroup = string.IsNullOrWhiteSpace(resourceGroupName)
    //                 ? null
    //                 : await resourceGroupStore.FindAsync(resourceGroupName);
    //
    //             var layerGroup = Activator.CreateInstance<LayerGroup>();
    //             layerGroup.Services = section.GetSection("ogcWebServices").Get<HashSet<ServiceType>>();
    //             layerGroup.Name = section.Key;
    //             layerGroup.ResourceGroup = resourceGroup;
    //             layerGroup.Layers = new List<Layer>();
    //             await RestoreAsync(layerGroup, section);
    //
    //             keys.Add(layerGroup.Name);
    //             Cache.AddOrUpdate(layerGroup.Name, layerGroup, (_, _) => layerGroup);
    //         }
    //     }
    //
    //     var removedKeys = existKeys.Except(keys);
    //     foreach (var removedKey in removedKeys)
    //     {
    //         Cache.TryRemove(removedKey, out _);
    //     }
    // }

    public async ValueTask<LayerGroup> FindAsync(string resourceGroupName, string layerGroupName)
    {
        if (string.IsNullOrEmpty(resourceGroupName))
        {
            return null;
        }

        var layerGroup = await FindAsync(layerGroupName);

        if (layerGroup == null || layerGroup.ResourceGroup?.Name != resourceGroupName)
        {
            return null;
        }

        return layerGroup;
    }

    public ValueTask<LayerGroup> FindAsync(string layerGroupName)
    {
        return _cache.TryGetValue(layerGroupName, out var item)
            ? new ValueTask<LayerGroup>(item.Clone())
            : new ValueTask<LayerGroup>();
    }

    public ValueTask<List<LayerGroup>> GetAllAsync()
    {
        var items = _cache.Values.Select(x => x.Clone()).ToList();
        return new ValueTask<List<LayerGroup>>(items);
    }

    public ValueTask<List<LayerGroup>> GetListAsync(string resourceGroup)
    {
        var result = new List<LayerGroup>();
        if (string.IsNullOrEmpty(resourceGroup))
        {
            return new ValueTask<List<LayerGroup>>(result);
        }

        foreach (var value in _cache.Values)
        {
            if (value.ResourceGroup?.Name == resourceGroup)
            {
                result.Add(value.Clone());
            }
        }

        return new ValueTask<List<LayerGroup>>(result);
    }

    private async Task RestoreAsync(LayerGroup layerGroup, JObject section)
    {
        var layerNames = section["layers"]?.ToObject<HashSet<string>>();
        if (layerNames != null)
        {
            foreach (var layerName in layerNames)
            {
                var parts = layerName.Split(':');
                Layer layer = null;
                switch (parts.Length)
                {
                    case 1:
                        layer = await layerStore.FindAsync(layerName);
                        break;
                    case 2:
                        layer = await layerStore.FindAsync(parts[0], parts[1]);
                        break;
                    default:
                        Logger.Value.LogError("图层组 {LayerGroupName} 中的图层 {LayerName} 不存在", layerGroup.Name, layerName);
                        break;
                }

                if (layer != null)
                {
                    layerGroup.Layers.Add(layer);
                }
            }
        }
    }

    // private async Task RestoreAsync(LayerGroup layerGroup, IConfigurationSection section)
    // {
    //     var layerNames = section.GetSection("layers").Get<HashSet<string>>();
    //     if (layerNames != null)
    //     {
    //         foreach (var layerName in layerNames)
    //         {
    //             var parts = layerName.Split(':');
    //             Layer layer = null;
    //             switch (parts.Length)
    //             {
    //                 case 1:
    //                     layer = await layerStore.FindAsync(null, layerName);
    //                     break;
    //                 case 2:
    //                     layer = await layerStore.FindAsync(parts[0], parts[1]);
    //                     break;
    //                 default:
    //                     Logger.LogError("图层组 {LayerGroupName} 中的图层 {LayerName} 不存在", layerGroup.Name, layerName);
    //                     break;
    //             }
    //
    //             if (layer != null)
    //             {
    //                 layerGroup.Layers.Add(layer);
    //             }
    //         }
    //     }
    // }
}