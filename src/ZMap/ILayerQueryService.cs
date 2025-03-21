

namespace ZMap;

/// <summary>
/// SCOPE
/// </summary>
public interface ILayerQueryService
{
    Task<(List<Layer> Layers, int FetchCount)> GetLayersAsync(List<LayerQuery> queryList, string traceIdentifier);
}