using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZMap;
using ZMap.Ogc;
using ZMap.Source;
using ZMap.Store;

namespace ZServer.Store;

public class LayerQueryService(
    ILayerGroupStore layerGroupStore,
    ILayerStore layerStore,
    ILogger<LayerQueryService> logger)
    : ILayerQueryService
{
    public async Task<List<Layer>> GetLayersAsync(
        List<LayerQuery> queryLayerRequests, string traceIdentifier)
    {
        var list = new List<Layer>();
        var hashSet = new HashSet<string>();

        foreach (var layerQuery in queryLayerRequests)
        {
            if (layerQuery.ResourceGroup == null)
            {
                var layer = await layerStore.FindAsync(null, layerQuery.Layer);
                if (layer != null)
                {
                    TryAdd(hashSet, list, layerQuery, layer);
                    continue;
                }

                logger.LogError("[{TraceIdentifier}] 图层 {Layer} 不存在", traceIdentifier, layerQuery.Layer);
            }
            else
            {
                var layerGroup = await layerGroupStore.FindAsync(layerQuery.ResourceGroup, layerQuery.Layer);
                if (layerGroup != null)
                {
                    foreach (var layer in layerGroup.Layers)
                    {
                        TryAdd(hashSet, list, layerQuery, layer);
                    }

                    continue;
                }
                else
                {
                    var layer = await layerStore.FindAsync(layerQuery.ResourceGroup, layerQuery.Layer);
                    if (layer != null)
                    {
                        TryAdd(hashSet, list, layerQuery, layer);
                        continue;
                    }
                }

                logger.LogError("[{TraceIdentifier}] 图层 {ResourceGroup}:{Layer} 不存在", traceIdentifier,
                    layerQuery.ResourceGroup, layerQuery.Layer);
            }
        }

        return list;
    }

    private void TryAdd(ISet<string> hashSet, ICollection<Layer> list, LayerQuery layerQuery, Layer layer)
    {
        if (!hashSet.Add(layer.Name))
        {
            return;
        }

        list.Add(layer);

        if (layer.Source is not IVectorSource vectorSource)
        {
            return;
        }

        if (layerQuery.Arguments == null || !layerQuery.Arguments.ContainsKey(
                                             Defaults.AdditionalFilter)
                                         || layerQuery.Arguments[Defaults.AdditionalFilter] == null)
        {
            return;
        }

        var filter = layerQuery.Arguments[Defaults.AdditionalFilter].ToString();
        vectorSource.Filter = filter;
    }
}