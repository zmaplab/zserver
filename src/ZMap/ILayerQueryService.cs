using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Ogc;

namespace ZMap;

/// <summary>
/// SCOPE
/// </summary>
public interface ILayerQueryService
{
    Task<List<Layer>> GetLayersAsync(List<LayerQuery> queryList, string traceIdentifier);
}