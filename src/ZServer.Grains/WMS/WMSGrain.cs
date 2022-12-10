using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using ZMap;
using ZMap.Source;
using ZMap.Utilities;
using ZServer.Extensions;
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
        private readonly ILogger<WMSGrain> _logger;
        private readonly ILayerQuerier _layerQuerier;
        private readonly IGraphicsServiceProvider _graphicsServiceProvider;

        public WMSGrain(ILogger<WMSGrain> logger,
            ILayerQuerier layerQuerier,
            IGraphicsServiceProvider graphicsServiceProvider)
        {
            _logger = logger;
            _layerQuerier = layerQuerier;
            _graphicsServiceProvider = graphicsServiceProvider;
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
            var traceIdentifier = extras.GetTraceIdentifier();
            var displayUrl =
                $"[{traceIdentifier}] LAYERS={layers}&FORMAT={format}&SRS={srs}&BBOX={bbox}&WIDTH={width}&HEIGHT={height}&FILTER={cqlFilter}&FORMAT_OPTIONS={formatOptions}";
            try
            {
                var modeState =
                    ModeStateUtilities.VerifyWmsGetMapArguments(layers, srs, bbox, width, height, format);
                if (!modeState.IsValid)
                {
                    return MapResult.Failed(modeState.Text, modeState.Code);
                }

                var dpi = 96;
                if (!string.IsNullOrWhiteSpace(formatOptions))
                {
                    var match = Regex.Match(formatOptions, "dpi:\\d+");
                    if (match.Success)
                    {
                        int.TryParse(match.Value.Replace("dpi:", ""), out dpi);
                    }
                }

                var filters = string.IsNullOrWhiteSpace(cqlFilter)
                    ? Array.Empty<string>()
                    : cqlFilter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                var layerQueries =
                    new List<QueryLayerParams>();

                for (var i = 0; i < modeState.Layers.Count; ++i)
                {
                    layerQueries.Add(new QueryLayerParams(modeState.Layers[i].ResourceGroup, modeState.Layers[i].Layer,
                        new Dictionary<string, object>
                        {
                            { Constants.AdditionalFilter, filters.ElementAtOrDefault(i) }
                        }));
                }

                var layerList = await _layerQuerier.GetLayersAsync(layerQueries, traceIdentifier);
                if (layerList.Count == 0)
                {
                    return MapResult.Ok(Array.Empty<byte>(), format);
                }

                var envelope = new Envelope(modeState.MinX, modeState.MaxX, modeState.MinY, modeState.MaxY);

                var viewPort = new Viewport
                {
                    Extent = envelope,
                    Width = width,
                    Height = height,
                    Transparent = transparent,
                    Bordered = extras.TryGetValue("Bordered", out var b) && (bool)b,
                    BackgroundColor = bgColor
                };

                var scale = GeoUtilities.CalculateOGCScale(envelope, modeState.SRID, width, dpi);
                var map = new Map();
                map.SetId(traceIdentifier)
                    .SetSRID(modeState.SRID)
                    .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                    .SetLogger(_logger)
                    .SetGraphicsContextFactory(_graphicsServiceProvider)
                    .AddLayers(layerList);
                var image = await map.GetImageAsync(viewPort, format);

                return MapResult.Ok(image, format);
            }
            catch (Exception e)
            {
                var msg = $"{displayUrl}, {e}";
                _logger.LogError(msg);
                return MapResult.Failed(e.Message, "InternalError");
            }
        }

        public Task<MapResult> GetCapabilitiesAsync(string version, string format,
            IDictionary<string, object> arguments)
        {
            return Task.FromResult(new MapResult());
        }

        public async Task<FeatureCollection> GetFeatureInfoAsync(string layers, string infoFormat, int featureCount,
            string srs,
            string bbox, int width, int height,
            double x, double y, IDictionary<string, object> arguments)
        {
            var traceIdentifier = arguments.GetTraceIdentifier();
            var displayUrl =
                $"[{traceIdentifier}] LAYERS={layers}&FORMAT={infoFormat}&SRS={srs}&BBOX={bbox}&WIDTH={width}&HEIGHT={height}";

            try
            {
                var modeState =
                    ModeStateUtilities.VerifyWmsGetFeatureInfoArguments(layers, srs, bbox, width, height, x, y,
                        featureCount);
                if (!modeState.IsValid)
                {
                    return new FeatureCollection();
                }

                var layerQueries =
                    new List<QueryLayerParams>();

                for (var i = 0; i < modeState.Layers.Count; ++i)
                {
                    layerQueries.Add(
                        new QueryLayerParams(modeState.Layers[i].ResourceGroup, modeState.Layers[i].Layer));
                }

                var layerList = await _layerQuerier.GetLayersAsync(layerQueries, traceIdentifier);
                var envelope = new Envelope(modeState.MinX, modeState.MaxX, modeState.MinY, modeState.MaxY);
                var featureCollection = await
                    GetFeatureInfoAsync(layerList, featureCount, srs, envelope, width, height, x, y);
                _logger.LogInformation(
                    $"Query features {displayUrl}, target layers: {string.Join(", ", layerList.Select(z => z.Name))}, count: {featureCollection.Count}");
                return featureCollection;
            }
            catch (Exception e)
            {
                var msg = $"{displayUrl}, {e}";
                _logger.LogError(msg);
                return new FeatureCollection();
            }
        }

        private async Task<FeatureCollection> GetFeatureInfoAsync(List<ILayer> layers,
            int featureCount,
            string srs,
            Envelope bbox, int width, int height, double x, double y)
        {
            if (layers == null || layers.Count == 0)
            {
                return new FeatureCollection();
            }

            var pixelSensitivity = 10;

            var pixelHeight = (bbox.MaxY - bbox.MinY) / height;
            var pixelWidth = (bbox.MaxX - bbox.MinX) / width;

            var latLon = GeoUtilities.CalculateLatLongFromGrid(bbox, pixelWidth, pixelHeight, (int)x, (int)y);

            var minX = latLon.Lon - pixelSensitivity * pixelWidth;
            var maxX = latLon.Lon + pixelSensitivity * pixelWidth;
            var minY = latLon.Lat - pixelSensitivity * pixelHeight;
            var maxY = latLon.Lat + pixelSensitivity * pixelHeight;

            var featureCollection = new FeatureCollection();
            var totalCount = 0;
            var srid = int.Parse(srs.Replace("EPSG:", ""));
            foreach (var layer in layers)
            {
                var queryCount = totalCount >= featureCount ? 0 : featureCount - totalCount;

                if (queryCount <= 0)
                {
                    break;
                }

                if (layer.Source is not SpatialDatabaseSource spatialDatabaseSource)
                {
                    continue;
                }

                var envelope = new Envelope(minX, maxX, minY, maxY);
                var targetEnvelope = envelope.Transform(srid, spatialDatabaseSource.SRID);
                var features = await spatialDatabaseSource
                    .GetFeaturesInExtentAsync(targetEnvelope);

                foreach (var feature in features)
                {
                    var attributes = new AttributesTable(feature.GetAttributes())
                    {
                        { "___layer", layer.Name }
                    };
                    featureCollection.Add(
                        new NetTopologySuite.Features.Feature(feature.Geometry, attributes));
                    totalCount++;
                    if (totalCount >= featureCount)
                    {
                        break;
                    }
                }
            }

            return featureCollection;
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