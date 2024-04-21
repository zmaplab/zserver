using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using ZMap.Extensions;
using ZMap.Ogc.Wmts;
using ZServer.Interfaces;
using ZServer.Interfaces.WMTS;

namespace ZServer.Grains.WMTS;

// ReSharper disable once InconsistentNaming
public class WMTSGrain(WmtsService wmtsService) : Grain, IWMTSGrain
{
    public async ValueTask<ZServerResponse> GetTileAsync(string layers, string styles, string format,
        string tileMatrixSet,
        string tileMatrix, int tileRow,
        int tileCol, string cqlFilter, IDictionary<string, object> arguments)
    {
        var result = await wmtsService.GetTileAsync(layers, styles, format, tileMatrixSet, tileMatrix, tileRow,
            tileCol, cqlFilter,
            arguments);
        if (!result.IsSuccess())
        {
            return ZServerResponseFactory.Failed(result.Message, result.Code);
        }

        await using var stream = result.Stream;
        var bytes = await result.Stream.ToArrayAsync();
        return ZServerResponseFactory.Ok(bytes, format);
    }
}