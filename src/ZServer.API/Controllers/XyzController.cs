using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using SkiaSharp;
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
[Microsoft.AspNetCore.Components.Route("[controller]")]
public class XyzController(ILogger<XyzController> logger, IClusterClient clusterClient)
    : ControllerBase
{
    /// <summary>
    /// 90% xyz 都是默认 3857
    /// </summary>
    /// <param name="layers"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="filter"></param>
    /// <param name="format"></param>
    /// <param name="tileMatrixSet"></param>
    /// <param name="style"></param>
    /// <param name="bordered"></param>
    [HttpGet("{layers}")]
    public async Task GetAsync([FromRoute, Required, StringLength(100)] string layers,
        [FromQuery] int x, [FromQuery] int y, [FromQuery, StringLength(100)] string z,
        [FromQuery(Name = "Z_FILTER"), StringLength(2048)]
        string filter = null,
        [StringLength(20)] string format = "image/png", [StringLength(12)] string tileMatrixSet = "3857",
        [StringLength(255)] string style = null, bool bordered = false)
    {
        tileMatrixSet = $"EPSG:{tileMatrixSet}";
        var tileMatrix = z;
        var tileCol = x;
        var tileRow = y;
        var tuple = Utility.GetWmtsPath(layers, filter, format, tileMatrixSet, tileMatrix, tileRow, tileCol, bordered);

        logger.LogDebug("[{TraceIdentifier}] Request wmts service {TileMatrix} {TileMatrixSet}  {TileCol}  {TileRow}",
            HttpContext.TraceIdentifier, tileMatrix, tileMatrixSet, tileCol, tileRow);

#if !DEBUG
        if (System.IO.File.Exists(tuple.FullPath))
        {
            if (EnvironmentVariables.EnableSensitiveDataLogging)
            {
                var displayUrl =
                    $"[{HttpContext.TraceIdentifier}] LAYERS={layers}&STYLES={style}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";
                logger.LogInformation("{Service} [{TraceIdentifier}] {Url}, CACHED", "XYZ", HttpContext.TraceIdentifier,
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
                    { Defaults.TraceIdentifier, HttpContext.TraceIdentifier },
                    { "Bordered", bordered }
                });

        await HttpContext.WriteZServerResponseAsync(result);
    }

    private SKEncodedImageFormat GetImageFormat(string format)
    {
        return format switch
        {
            "image/png" => SKEncodedImageFormat.Png,
            "image/jpeg" => SKEncodedImageFormat.Jpeg,
            "image/webp" => SKEncodedImageFormat.Webp,
            "image/gif" => SKEncodedImageFormat.Gif,
            "image/bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Png
        };
    }
}