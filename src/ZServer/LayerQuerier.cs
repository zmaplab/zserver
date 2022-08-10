using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZMap;
using ZMap.Source;
using ZServer.Store;

namespace ZServer;

public class LayerQuerier : ILayerQuerier
{
    private readonly ILayerGroupStore _layerGroupStore;
    private readonly ILayerStore _layerStore;
    private readonly ILogger<LayerQuerier> _logger;

    public LayerQuerier(ILayerGroupStore layerGroupStore, ILayerStore layerStore, ILogger<LayerQuerier> logger)
    {
        _layerGroupStore = layerGroupStore;
        _layerStore = layerStore;
        _logger = logger;
    }

    public async Task<List<ILayer>> GetLayersAsync(
        List<QueryLayerParams> queryLayerRequests, string traceIdentifier)
    {
        var list = new List<ILayer>();
        var hashSet = new HashSet<string>();

        foreach (var layerQuery in queryLayerRequests)
        {
            if (layerQuery.ResourceGroup == null)
            {
                var layer = await _layerStore.FindAsync(layerQuery.Layer);
                if (layer != null)
                {
                    TryAdd(hashSet, list, layerQuery, layer);
                    continue;
                }

                var msg =
                    $"[{traceIdentifier}] Layer {layerQuery.Layer} is not exists";
                _logger.LogError(msg);
            }
            else
            {
                var layerGroup = await _layerGroupStore.FindAsync(layerQuery.ResourceGroup, layerQuery.Layer);
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
                    var layer = await _layerStore.FindAsync(layerQuery.ResourceGroup, layerQuery.Layer);
                    if (layer != null)
                    {
                        TryAdd(hashSet, list, layerQuery, layer);
                        continue;
                    }
                }

                var msg =
                    $"[{traceIdentifier}] Layer {layerQuery.ResourceGroup}:{layerQuery.Layer} is not exists";
                _logger.LogError(msg);
            }
        }

        return list;
    }

    private void TryAdd(ISet<string> hashSet, ICollection<ILayer> list, QueryLayerParams layerQuery, ILayer layer)
    {
        if (hashSet.Contains(layer.Name))
        {
            return;
        }
        else
        {
            hashSet.Add(layer.Name);
            list.Add(layer);
        }

        if (layer.Source is not IVectorSource vectorSource)
        {
            return;
        }

        if (layerQuery.Arguments == null || !layerQuery.Arguments.ContainsKey(
                                             Constants.AdditionalFilter)
                                         || layerQuery.Arguments[Constants.AdditionalFilter] == null)
        {
            return;
        }

        var filter = layerQuery.Arguments[Constants.AdditionalFilter].ToString();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            vectorSource.SetFilter(new CQLFilter(filter));
        }
    }
}