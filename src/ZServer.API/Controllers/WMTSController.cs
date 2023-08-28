using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using ZServer.Interfaces.WMTS;

namespace ZServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // ReSharper disable once InconsistentNaming
    public class WMTSController : ControllerBase
    {
        private readonly Lazy<IClusterClient> _clusterClient;

        public WMTSController(Lazy<IClusterClient> clusterClient)
        {
            _clusterClient = clusterClient;
        }

        /// <summary>
        /// 支持将多个 layer 合并成一个图层
        /// workspace1:layer1,workspace2:layer2
        /// 缓存路径： workspace1.layer1_workspace2.layer2
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="style"></param>
        /// <param name="tileMatrix"></param>
        /// <param name="tileRow"></param>
        /// <param name="tileCol"></param>
        /// <param name="format"></param>
        /// <param name="tileMatrixSet"></param>
        /// <param name="cqlFilter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task GetAsync([Required] string layer, string style, [Required] string tileMatrix,
            [Required] int tileRow, [Required] int tileCol, string format = "image/png",
            string tileMatrixSet = "EPSG:4326", [FromQuery(Name = "CQL_FILTER")] string cqlFilter = null)
        {
            var keyBuilder = new StringBuilder();

            var layers = layer.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
            layers.Sort();

            keyBuilder.Append(string.Join("_", layers));
            keyBuilder.Append(tileMatrixSet).Append('/');
            keyBuilder.Append(tileMatrix).Append('/');
            keyBuilder.Append(tileRow).Append('/');
            keyBuilder.Append(tileCol);

            var key = keyBuilder.ToString();

            var friend = _clusterClient.Value.GetGrain<IWMTSGrain>(key);
            var result =
                await friend.GetTileAsync(layer, style, format, tileMatrixSet, tileMatrix, tileRow, tileCol,
                    cqlFilter,
                    new Dictionary<string, object>
                    {
                        { "TraceIdentifier", HttpContext.TraceIdentifier }
                    });

            await HttpContext.WriteMapImageAsync(result);
        }
    }
}