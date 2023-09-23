using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using ZMap.Extensions;
using ZMap.Ogc.Wmts;
using ZServer.Interfaces;
using ZServer.Interfaces.WMTS;

namespace ZServer.Grains.WMTS
{
    // ReSharper disable once InconsistentNaming
    public class WMTSGrain : Grain, IWMTSGrain
    {
        private readonly WmtsService _wmtsService;

        public WMTSGrain(WmtsService wmtsService)
        {
            _wmtsService = wmtsService;
        }

        public async ValueTask<MapResult> GetTileAsync(string layers, string styles, string format,
            string tileMatrixSet,
            string tileMatrix, int tileRow,
            int tileCol, string cqlFilter, IDictionary<string, object> arguments)
        {
            var result = await _wmtsService.GetTileAsync(layers, styles, format, tileMatrixSet, tileMatrix, tileRow,
                tileCol, cqlFilter,
                arguments);
            if (!string.IsNullOrEmpty(result.Code))
            {
                return MapResult.Failed(result.Message, result.Code);
            }

            await using var stream = result.Image;
            var bytes = await result.Image.ToArrayAsync();
            return MapResult.Ok(bytes, format);
        }
    }
}