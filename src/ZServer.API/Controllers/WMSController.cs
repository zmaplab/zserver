using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orleans;
using ZMap;
using ZMap.Ogc.Wms;
using ZServer.Interfaces;
using ZServer.Interfaces.WMS;

namespace ZServer.API.Controllers;

/// <summary>
/// WMTS 服务
/// </summary>
/// <param name="clusterClient"></param>
/// <param name="options"></param>
/// <param name="wmsService"></param>
[ApiController]
[Route("[controller]")]
[Authorize(Policy = "default")]
// ReSharper disable once InconsistentNaming
public class WMSController(
    IClusterClient clusterClient,
    IOptions<ServerOptions> options,
    WmsService wmsService) : ControllerBase
{
    // private static readonly Random Rnd = new();

    /// <summary>
    /// WMS 服务接口
    /// </summary>
    /// <param name="request"></param>
    /// <param name="layers">图层名称, 支持两种方式
    /// 1. 资源组图层: resourceGroup:layer
    /// 2. 直接访问图层: layer
    /// 通过 , 号分隔可以同时请求多个图层 resourceGroup1:layer1,resourceGroup1:layer2
    /// </param>
    /// <param name="styles">图层渲染样式名称列表（用逗号分隔，与 LAYERS 顺序对应，默认用默认样式）</param>
    /// <param name="srs">前端空间参考系统, EPSG:900913</param>
    /// <param name="bbox">地图地理范围（minX,minY,maxX,maxY）: 123.3984375,27.7734375,126.9140625,31.2890625</param>
    /// <param name="width">输出地图图像的宽度</param>
    /// <param name="height">输出地图图像的高度</param>
    /// <param name="format">输出图像格式，一般使用 PNG</param>
    /// <param name="transparent"></param>
    /// <param name="bgColor">图片背景色</param>
    /// <param name="time">请求的时间搓</param>
    /// <param name="filter">Z_FILTER，编写 SQL 后面的 WHERE 限制条件</param>
    /// <param name="formatOptions">格式化参数，参考 WMS 标准</param>
    /// <param name="featureCount"></param>
    /// <param name="exceptions">异常格式：XML 或者 JSON</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="buffer"></param>
    /// <param name="bordered"></param>
    [HttpGet]
    public async Task GetAsync(
        [StringLength(25), Required] string request,
        [StringLength(255), Required] string layers,
        [StringLength(255)] string styles,
        [StringLength(15), Required] string srs,
        [StringLength(100), Required] string bbox,
        int width,
        int height,
        [StringLength(25)] string format,
        bool transparent,
        [StringLength(25)] string bgColor,
        int time,
        [FromQuery(Name = "Z_FILTER"), StringLength(2048)]
        string filter,
        [FromQuery(Name = "FORMAT_OPTIONS"), StringLength(100)]
        string formatOptions,
        [StringLength(255)] string exceptions,
        float x, float y, int featureCount = 1, int buffer = 0, bool bordered = false)
    {
        // var layerList = layers.Split(',', StringSplitOptions.RemoveEmptyEntries);
        // foreach (var layer in layerList)
        // {
        //     var permission = await permissionService.EnforceAsync("read", layer, PolicyEffect.Allow);
        //     if (permission)
        //     {
        //         continue;
        //     }
        //
        //     HttpContext.Response.StatusCode = 403;
        //     return;
        // }

        switch (request)
        {
            case "GetMap":
            {
                if (options.Value.WmsCluster)
                {
                    // comments by lewis at 20230923
                    // 还是应该使用集群模式， 不然无法分布式分担渲染、查询开销， 要依赖外部负载组件
                    var grain = clusterClient.GetGrain<IWMSGrain>(0);
                    var response =
                        await grain.GetMapAsync(layers, styles, srs, bbox, width, height, format,
                            transparent, bgColor, time,
                            formatOptions, filter,
                            new Dictionary<string, object>
                            {
                                { Defaults.TraceIdentifier, HttpContext.TraceIdentifier },
                                { "Bordered", bordered },
                                { "Buffer", buffer }
                            });
                    await HttpContext.WriteZServerResponseAsync(response);
                }
                else
                {
                    using var result = await wmsService.GetMapAsync(layers, styles, srs, bbox, width, height, format,
                        transparent, bgColor, time,
                        formatOptions, filter, new Dictionary<string, object>
                        {
                            { Defaults.TraceIdentifier, HttpContext.TraceIdentifier },
                            { "Bordered", bordered },
                            { "Buffer", buffer }
                        });
                    if (!result.IsSuccess())
                    {
                        var response = ZServerResponseFactory.Failed(result.Message, result.Code, result.Locator);
                        await HttpContext.WriteZServerResponseAsync(response);
                        break;
                    }

                    await using var stream = result.Stream;
                    // var bytes = result.Stream.ToArray();
                    // response = ZServerResponseFactory.Ok(bytes, format);
                    HttpContext.Response.ContentType = format;
                    HttpContext.Response.ContentLength = stream.Length;
                    await stream.CopyToAsync(HttpContext.Response.Body, (int)stream.Length);
                }

                break;
            }
            case "GetFeatureInfo":
            {
                if (options.Value.WmsCluster)
                {
                    var friend = clusterClient.GetGrain<IWMSGrain>(0);
                    var result =
                        await friend.GetFeatureInfoAsync(layers, srs, bbox, width, height, null, featureCount, x, y,
                            new Dictionary<string, object>
                            {
                                { Defaults.TraceIdentifier, HttpContext.TraceIdentifier }
                            });
                    await HttpContext.WriteAsync(result);
                }
                else
                {
                    var result = await wmsService.GetFeatureInfoAsync(layers, srs, bbox, width, height, null,
                        featureCount, x, y,
                        new Dictionary<string, object>
                        {
                            { Defaults.TraceIdentifier, HttpContext.TraceIdentifier }
                        });

                    if (!string.IsNullOrEmpty(result.Code))
                    {
                        var response = ZServerResponseFactory.Failed(result.Message, result.Code);
                        await HttpContext.WriteZServerResponseAsync(response);
                        break;
                    }

                    await HttpContext.WriteAsync(result.Features);
                }

                break;
            }
            default:
            {
                HttpContext.Response.StatusCode = 404;
                return;
            }
        }
    }

    // private int GetLoadBalanceKey()
    // {
    //     var wmsActivationNum = configuration["wmsActivationNum"];
    //     return Rnd.Next(string.IsNullOrEmpty(wmsActivationNum) ? 96 : int.Parse(wmsActivationNum));
    // }
}