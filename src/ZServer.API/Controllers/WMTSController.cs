using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using ZMap;
using ZMap.Infrastructure;
using ZServer.Interfaces.WMTS;

namespace ZServer.API.Controllers;

/// <summary>
/// WMS 服务
/// </summary>
/// <param name="clusterClient"></param>
/// <param name="logger"></param>
[ApiController]
[Route("[controller]")]
[Authorize(Policy = "default")]
// ReSharper disable once InconsistentNaming
public class WMTSController(
    IClusterClient clusterClient,
    ILogger<WMTSController> logger
)
    : ControllerBase
{
    /// <summary>
    /// 
    /// 支持将多个 layer 合并成一个图层
    /// workspace1:layer1,workspace2:layer2
    /// 缓存路径： workspace1.layer1_workspace2.layer2
    /// </summary>
    /// <param name="layers">需渲染的图层名称列表（用逗号分隔，名称需与服务元数据一致）</param>
    /// <param name="style">图层渲染样式名称列表（用逗号分隔，与 LAYERS 顺序对应，默认用默认样式）</param>
    /// <param name="tileMatrix"></param>
    /// <param name="tileRow"></param>
    /// <param name="tileCol"></param>
    /// <param name="format"></param>
    /// <param name="tileMatrixSet"></param>
    /// <param name="filter"></param>
    /// <param name="bordered"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task GetAsync([Required] [FromQuery(Name = "layer"), StringLength(100)] string layers,
        [StringLength(255)] string style,
        [Required, StringLength(100)] string tileMatrix, [Required] int tileRow, [Required] int tileCol,
        [StringLength(25)]
        string format = "image/png",
        [Required, StringLength(50)] string tileMatrixSet = "EPSG:4326",
        [FromQuery(Name = "Z_FILTER"), StringLength(2048)]
        string filter = null, bool bordered = false)
    {
        var tuple = Utility.GetWmtsPath(layers, filter, format, tileMatrixSet, tileMatrix, tileRow, tileCol);

        logger.LogDebug("[{TraceIdentifier}] Request wmts service {TileMatrix} {TileMatrixSet}  {TileCol}  {TileRow}",
            HttpContext.TraceIdentifier, tileMatrix, tileMatrixSet, tileCol, tileRow);

#if !DEBUG
        if (System.IO.File.Exists(tuple.FullPath))
        {
            if (EnvironmentVariables.EnableSensitiveDataLogging)
            {
                var displayUrl =
                    $"[{HttpContext.TraceIdentifier}] LAYERS={layers}&STYLES={style}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";
                logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", HttpContext.TraceIdentifier, displayUrl);
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
                    { Defaults.TraceIdentifier, HttpContext.TraceIdentifier },
                    { "Bordered", bordered }
                });

        await HttpContext.WriteZServerResponseAsync(result);
    }
}