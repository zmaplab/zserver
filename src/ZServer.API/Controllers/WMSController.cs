using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Orleans;
using ZServer.Interfaces.WMS;

namespace ZServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // ReSharper disable once InconsistentNaming
    public class WMSController : ControllerBase
    {
        private static readonly Random Rnd = new Random();
        private readonly IClusterClient _clusterClient;
        private readonly IConfiguration _configuration;

        public WMSController(IClusterClient clusterClient, IConfiguration configuration)
        {
            _clusterClient = clusterClient;
            _configuration = configuration;
        }

        /// <summary>
        /// WMS 服务接口
        /// </summary>
        /// <param name="request"></param>
        /// <param name="layers">图层名称, 支持两种方式
        /// 1. 资源组图层: resourceGroup:layer
        /// 2. 直接访问图层: layer
        /// 通过 , 号分隔可以同时请求多个图层 resourceGroup1:layer1,resourceGroup1:layer2
        /// </param>
        /// <param name="styles">前端传入样式，暂不支持</param>
        /// <param name="srs">前端投影标识</param>
        /// <param name="bbox">渲染区域</param>
        /// <param name="width">渲染图片的宽度</param>
        /// <param name="height">渲染图片的高度</param>
        /// <param name="format">返回图片的格式，一般使用 PNG</param>
        /// <param name="transparent"></param>
        /// <param name="bgColor">图片背景色</param>
        /// <param name="time">请求的时间搓</param>
        /// <param name="filter">CQL_FILTER，编写 SQL 后面的 WHERE 限制条件</param>
        /// <param name="formatOptions">格式化参数，参考 WMS 标准</param>
        /// <param name="featureCount"></param>
        /// <param name="exceptions">异常格式：XML 或者 JSON</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buffer"></param>
        /// <param name="bordered"></param>
        [HttpGet]
        public async Task GetAsync(
            string request,
            string layers, string styles,
            string srs,
            string bbox,
            int width,
            int height,
            string format,
            bool transparent,
            string bgColor,
            int time,
            [FromQuery(Name = "CQL_FILTER")] string filter,
            [FromQuery(Name = "FORMAT_OPTIONS")] string formatOptions,
            string exceptions,
            double x, double y, int featureCount = 1, int buffer = 0, bool bordered = false)
        {
            switch (request)
            {
                case "GetMap":
                {
                    // comments by lewis at 20230923
                    // 还是应该使用集群模式， 不然无法分布式分担渲染、查询开销， 要依赖外部负载组件
                    var friend = _clusterClient.GetGrain<IWMSGrain>(GetLoadBalanceKey());
                    var result =
                        await friend.GetMapAsync(layers, styles, srs, bbox, width, height, format,
                            transparent, bgColor, time,
                            formatOptions, filter,
                            new Dictionary<string, object>
                            {
                                { "TraceIdentifier", HttpContext.TraceIdentifier },
                                { "Bordered", bordered },
                                { "Buffer", buffer }
                            });

                    // var result = await _wmsService.GetMapAsync(layers, styles, srs, bbox, width, height, format,
                    //     transparent, bgColor, time,
                    //     formatOptions, filter, new Dictionary<string, object>
                    //     {
                    //         { "TraceIdentifier", HttpContext.TraceIdentifier },
                    //         { "Bordered", bordered },
                    //         { "Buffer", buffer }
                    //     });

                    await HttpContext.WriteResultAsync(result);
                    break;
                }
                case "GetFeatureInfo":
                {
                    var friend = _clusterClient.GetGrain<IWMSGrain>(GetLoadBalanceKey());
                    var result =
                        await friend.GetFeatureInfoAsync(layers, null, featureCount, srs, bbox, width, height, x, y,
                            new Dictionary<string, object>
                            {
                                { "TraceIdentifier", HttpContext.TraceIdentifier }
                            });
                    await HttpContext.WriteAsync(result);
                    break;
                }
                default:
                {
                    HttpContext.Response.StatusCode = 404;
                    return;
                }
            }
        }

        private int GetLoadBalanceKey()
        {
            var wmsActivationNum = _configuration["wmsActivationNum"];
            return Rnd.Next(string.IsNullOrEmpty(wmsActivationNum) ? 96 : int.Parse(wmsActivationNum));
        }
    }
}