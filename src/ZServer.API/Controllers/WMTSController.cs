using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using ZMap.Infrastructure;
using ZServer.Interfaces;
using ZServer.Interfaces.WMTS;

namespace ZServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // ReSharper disable once InconsistentNaming
    public class WMTSController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;
        private readonly ILogger<WMTSController> _logger;

        public WMTSController(IClusterClient clusterClient, ILogger<WMTSController> logger)
        {
            _clusterClient = clusterClient;
            _logger = logger;
        }

        /// <summary>
        /// 支持将多个 layer 合并成一个图层
        /// workspace1:layer1,workspace2:layer2
        /// 缓存路径： workspace1.layer1_workspace2.layer2
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="style"></param>
        /// <param name="tileMatrix"></param>
        /// <param name="tileRow"></param>
        /// <param name="tileCol"></param>
        /// <param name="format"></param>
        /// <param name="tileMatrixSet"></param>
        /// <param name="cqlFilter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task GetAsync([Required] [FromQuery(Name = "layer")] string layers, string style,
            [Required] string tileMatrix,
            [Required] int tileRow, [Required] int tileCol, string format = "image/png",
            string tileMatrixSet = "EPSG:4326", [FromQuery(Name = "CQL_FILTER")] string cqlFilter = null)
        {
            var path = Utilities.GetWmtsKey(layers, cqlFilter, format, tileMatrixSet, tileRow.ToString(),
                tileCol.ToString());
            if (System.IO.File.Exists(path))
            {
                var displayUrl =
                    $"[{HttpContext.TraceIdentifier}] LAYERS={layers}&STYLES={style}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";
                _logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", HttpContext.TraceIdentifier,
                    displayUrl);
                await HttpContext.WriteResultAsync(MapResult.Ok(await System.IO.File.ReadAllBytesAsync(path), format));
                return;
            }

            var keyBuilder = new StringBuilder();

            foreach (var layer in layers.Split(",", StringSplitOptions.RemoveEmptyEntries).Order())
            {
                keyBuilder.Append(layer);
            }

            keyBuilder.Append('/');
            keyBuilder.Append(tileMatrixSet).Append('/');
            keyBuilder.Append(tileMatrix).Append('/');
            keyBuilder.Append(tileRow).Append('/');
            keyBuilder.Append(tileCol);

            var key = keyBuilder.ToString();

            var friend = _clusterClient.GetGrain<IWMTSGrain>(key);
            var result =
                await friend.GetTileAsync(layers, style, format, tileMatrixSet, tileMatrix, tileRow, tileCol,
                    cqlFilter,
                    new Dictionary<string, object>
                    {
                        { "TraceIdentifier", HttpContext.TraceIdentifier }
                    });

            await HttpContext.WriteResultAsync(result);
        }
    }
}