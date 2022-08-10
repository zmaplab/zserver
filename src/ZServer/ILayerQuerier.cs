using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap;

namespace ZServer;

public interface ILayerQuerier
{
    Task<List<ILayer>> GetLayersAsync(
        List<QueryLayerParams> queryLayerRequests, string traceIdentifier);
}