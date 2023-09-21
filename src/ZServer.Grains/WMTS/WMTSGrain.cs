using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<WMTSGrain> _logger;

        public WMTSGrain(WmtsService wmtsService, ILogger<WMTSGrain> logger)
        {
            _wmtsService = wmtsService;
            _logger = logger;
        }

        public async Task<MapResult> GetTileAsync(string layers, string styles, string format,
            string tileMatrixSet,
            string tileMatrix, int tileRow,
            int tileCol, string cqlFilter, IDictionary<string, object> arguments)
        {
            var result = await _wmtsService.GetTileAsync(layers, styles, format, tileMatrixSet, tileMatrix, tileRow,
                tileCol, cqlFilter,
                arguments);

            if (!string.IsNullOrEmpty(result.Code))
            {
                _logger.LogError(result.Code + ": " + result.Message);
                // return MapResult.Failed(result.Message, result.Code);
            }

            if (result.Stream == null)
            {
                return MapResult.Ok(Array.Empty<byte>(), format);
            }

            await using var stream = result.Stream;
            var bytes = await stream.ToArrayAsync();
            return MapResult.Ok(bytes, format);
        }
    }
}