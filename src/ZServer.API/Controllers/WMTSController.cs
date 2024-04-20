using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using ZMap;
using ZMap.Infrastructure;
using ZServer.Interfaces.WMTS;

namespace ZServer.API.Controllers;

[ApiController]
[Route("[controller]")]
// ReSharper disable once InconsistentNaming
public class WMTSController(IClusterClient clusterClient, ILogger<WMTSController> logger)
    : ControllerBase
{
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
        // 使用相同的缓存路径
        var path = Utilities.GetWmtsPath(layers, cqlFilter, format, tileMatrixSet, tileRow, tileCol);
        if (System.IO.File.Exists(path))
        {
            if (EnvironmentVariables.EnableSensitiveDataLogging)
            {
                var displayUrl =
                    $"[{HttpContext.TraceIdentifier}] LAYERS={layers}&STYLES={style}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";
                logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", HttpContext.TraceIdentifier,
                    displayUrl);
            }

            await using var stream = System.IO.File.OpenRead(path);
            HttpContext.Response.ContentType = format;
            HttpContext.Response.ContentLength = stream.Length;
            await stream.CopyToAsync(HttpContext.Response.Body, (int)stream.Length);
            return;
        }

        // 同一个 Grid 使用同一个对象进行管理， 保证缓存文件在同一个 Silo 目录下
        var key = Utilities.GetWmtsKey(layers, tileMatrixSet, tileRow, tileCol);
        var friend = clusterClient.GetGrain<IWMTSGrain>(key);
        var result =
            await friend.GetTileAsync(layers, style, format, tileMatrixSet, tileMatrix, tileRow, tileCol,
                cqlFilter,
                new Dictionary<string, object>
                {
                    { "TraceIdentifier", HttpContext.TraceIdentifier }
                });

        await HttpContext.WriteZServerResponseAsync(result);
    }
}