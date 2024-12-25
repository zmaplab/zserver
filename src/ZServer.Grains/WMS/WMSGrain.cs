using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using ZMap.Extensions;
using ZMap.Ogc.Wms;
using ZServer.Interfaces;
using ZServer.Interfaces.WMS;

namespace ZServer.Grains.WMS;

/// <summary>
/// 
/// </summary>
[Reentrant]
[StatelessWorker]
public class WmsGrain(WmsService wmsService) : Grain, IWMSGrain
{
    public ValueTask<ZServerResponse> GetCapabilitiesAsync(string version, string format,
        IDictionary<string, object> arguments)
    {
        return ValueTask.FromResult(new ZServerResponse());
    }

    /// <summary>
    /// 20, 500
    /// 19, 1000
    /// 18, 2000
    /// 17, 4000
    /// 16, 8000
    /// 15, 16000
    /// 14, 32000
    /// 13, 64000
    /// 12, 128000
    /// 11, 256000
    /// 10, 512000
    /// 9, 1024000
    /// 8, 2048000
    /// 7, 4096000
    /// 6, 8192000
    /// 5, 16384000
    /// 4, 32768000
    /// 3, 65536000
    /// 2, 131072000
    /// </summary>
    public async ValueTask<ZServerResponse> GetMapAsync(string layers, string styles, string srs, string bbox,
        int width, int height, string format, bool transparent, string bgColor, int time,
        string formatOptions, string cqlFilter, IDictionary<string, object> arguments)
    {
        using var result = await wmsService.GetMapAsync(layers, styles, srs, bbox, width, height, format,
            transparent, bgColor, time,
            formatOptions, cqlFilter, arguments);

        if (!result.IsSuccess())
        {
            return ZServerResponseFactory.Failed(result.Message, result.Code, result.Locator);
        }

        var stream = result.Stream;
        var bytes = await stream.ToArrayAsync();
        return ZServerResponseFactory.Ok(bytes, format);
    }

    public async ValueTask<ZServerResponse> GetFeatureInfoAsync(string layers, string srs, string bbox, int width,
        int height,
        string infoFormat, int featureCount,
        double x, double y, IDictionary<string, object> arguments)
    {
        var modeState = await wmsService.GetFeatureInfoAsync(layers, srs, bbox,
            width, height, infoFormat, featureCount,
            x, y, arguments);

        if (!string.IsNullOrEmpty(modeState.Code))
        {
            return ZServerResponseFactory.Ok(Encoding.UTF8.GetBytes(""), "application/json");
        }

        var json = GeoJsonSerializer.Serialize(modeState.Features);
        return ZServerResponseFactory.Ok(Encoding.UTF8.GetBytes(json), "application/json");
    }

    // public override async Task OnActivateAsync(CancellationToken cancellationToken)
    // {
    //     // await RegisterOrUpdateReminder(GetType().Name, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
    // }

    // public Task ReceiveReminder(string reminderName, TickStatus status)
    // {
    //     return Task.CompletedTask;
    // }
}