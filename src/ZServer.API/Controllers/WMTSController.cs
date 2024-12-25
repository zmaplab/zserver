using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using ZMap.Infrastructure;
using ZServer.Interfaces.WMTS;

namespace ZServer.API.Controllers;

[ApiController]
[Route("[controller]")]
[ZServerAuthorize]
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
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task GetAsync([Required]
        [FromQuery(Name = "layer"),
         StringLength(100)]
        string layers,
        [StringLength(100)] string style,
        [Required, StringLength(50)] string tileMatrix, [Required] int tileRow, [Required] int tileCol,
        string format = "image/png",
        [Required, StringLength(50)] string tileMatrixSet = "EPSG:4326",
        [FromQuery(Name = "Z_FILTER"), StringLength(2048)]
        string filter = null)
    {
        var tuple = Utility.GetWmtsPath(layers, filter, format, tileMatrixSet, tileMatrix, tileRow, tileCol);
#if !DEBUG
        if (System.IO.File.Exists(tuple.FullPath))
        {
            if (ZMap.EnvironmentVariables.EnableSensitiveDataLogging)
            {
                var displayUrl =
                    $"[{HttpContext.TraceIdentifier}] LAYERS={layers}&STYLES={style}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";
                logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", HttpContext.TraceIdentifier,
                    displayUrl);
            }

            await using var stream = System.IO.File.OpenRead(tuple.FullPath);
            HttpContext.Response.ContentType = format;
            HttpContext.Response.ContentLength = stream.Length;
            await stream.CopyToAsync(HttpContext.Response.Body, (int)stream.Length);
            return;
        }
#endif

        // 同一个 Grid 使用同一个对象进行管理， 保证缓存文件在同一个 Silo 目录下
        var grain = clusterClient.GetGrain<IWMTSGrain>(tuple.IntervalPath);
        var result =
            await grain.GetTileAsync(layers, style, format, tileMatrixSet, tileMatrix, tileRow, tileCol,
                filter,
                new Dictionary<string, object>
                {
                    { "TraceIdentifier", HttpContext.TraceIdentifier }
                });

        await HttpContext.WriteZServerResponseAsync(result);
    }
}