using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Ogc;

namespace ZMap.Store;

public interface ILayerQueryService
{
    Task<List<Layer>> GetLayersAsync(
        List<QueryLayerParams> paramList, string traceIdentifier);
}