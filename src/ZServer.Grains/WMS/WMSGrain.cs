using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using ZMap.Extensions;
using ZMap.Ogc.Wms;
using ZServer.Interfaces;
using ZServer.Interfaces.WMS;

namespace ZServer.Grains.WMS
{
    /// <summary>
    /// 
    /// </summary>
    [Reentrant, StatelessWorker]
    // ReSharper disable once InconsistentNaming
    public class WMSGrain : Grain, IWMSGrain
    {
        private readonly WmsService _wmsService;

        public WMSGrain(WmsService wmsService)
        {
            _wmsService = wmsService;
        }

        public Task<MapResult> GetCapabilitiesAsync(string version, string format,
            IDictionary<string, object> arguments)
        {
            return Task.FromResult(new MapResult());
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
        /// <param name="bgColor"></param>
        /// <param name="time"></param>
        /// <param name="layers">LAYERS</param>
        /// <param name="format">FORMAT</param>
        /// <param name="srs">SRS</param>
        /// <param name="bbox">BBOX</param>
        /// <param name="width">WIDTH</param>
        /// <param name="height">HEIGHT</param>
        /// <param name="cqlFilter">CQL_FILTER</param>
        /// <param name="styles"></param>
        /// <param name="formatOptions">FORMAT_OPTIONS</param>
        /// <param name="extras"></param>
        /// <param name="transparent"></param>
        /// <returns></returns>
        public async Task<MapResult> GetMapAsync(string layers, string styles, string srs, string bbox,
            int width, int height, string format, bool transparent, string bgColor, int time,
            string formatOptions, string cqlFilter, IDictionary<string, object> extras)
        {
            var result = await _wmsService.GetMapAsync(layers, styles, srs, bbox, width, height, format,
                transparent, bgColor, time,
                formatOptions, cqlFilter, extras);
            if (!string.IsNullOrEmpty(result.Code))
            {
                return MapResult.Failed(result.Message, result.Code);
            }

            await using var stream = result.Stream;
            var bytes = await result.Stream.ToArrayAsync();
            return MapResult.Ok(bytes, format);
        }

        public async Task<FeatureCollection> GetFeatureInfoAsync(string layers, string infoFormat, int featureCount,
            string srs,
            string bbox, int width, int height,
            double x, double y, IDictionary<string, object> arguments)
        {
            var modeState = await _wmsService.GetFeatureInfoAsync(layers, infoFormat, featureCount,
                srs, bbox, width, height,
                x, y, arguments);
            return !string.IsNullOrEmpty(modeState.Code) ? new FeatureCollection() : modeState.Features;
        }

        public override async Task OnActivateAsync()
        {
            await RegisterOrUpdateReminder(GetType().Name, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return Task.CompletedTask;
        }
    }
}