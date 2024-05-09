

namespace ZMap;

/// <summary>
/// SCOPE
/// </summary>
public interface ILayerQueryService
{
    Task<List<Layer>> GetLayersAsync(List<LayerQuery> queryList, string traceIdentifier);
}