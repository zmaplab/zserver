

namespace ZMap;

public interface ILayerQueryService
{
    Task<LayerQueryResult> GetLayersAsync(List<LayerQuery> queryList, string traceIdentifier);
}