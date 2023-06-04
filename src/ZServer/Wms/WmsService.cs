using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using ZMap;
using ZMap.Infrastructure;
using ZMap.Source;
using ZServer.Extensions;

namespace ZServer.Wms;

public class WmsService : IWmsService
{
    private readonly ILogger<WmsService> _logger;
    private readonly IGraphicsServiceProvider _graphicsServiceProvider;
    private readonly ILayerQuerier _layerQuerier;

    public WmsService(ILogger<WmsService> logger, IGraphicsServiceProvider graphicsServiceProvider,
        ILayerQuerier layerQuerier)
    {
        _logger = logger;
        _graphicsServiceProvider = graphicsServiceProvider;
        _layerQuerier = layerQuerier;
    }

    public async Task<(string Code, string Message, Stream Stream)> GetMapAsync(string layers, string styles,
        string srs, string bbox, int width,
        int height, string format,
        bool transparent, string bgColor, int time, string formatOptions, string cqlFilter,
        IDictionary<string, object> extras)
    {
        var traceIdentifier = extras.GetTraceIdentifier();
        var displayUrl =
            $"[{traceIdentifier}] LAYERS={layers}&FORMAT={format}&SRS={srs}&BBOX={bbox}&WIDTH={width}&HEIGHT={height}&FILTER={cqlFilter}&FORMAT_OPTIONS={formatOptions}";
        try
        {
            var modeState =
                ArgumentsValidator.VerifyAndBuildWmsGetMapArguments(layers, srs, bbox, width, height, format);
            if (!string.IsNullOrEmpty(modeState.Code))
            {
                _logger.LogError("{Url}, arguments error", displayUrl);
                return (modeState.Code, modeState.Message, null);
            }

            var dpi = Utilities.GetDpi(formatOptions);

            var filters = string.IsNullOrWhiteSpace(cqlFilter)
                ? Array.Empty<string>()
                : cqlFilter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var layerQueries =
                new List<QueryLayerParams>();

            for (var i = 0; i < modeState.Arguments.Layers.Count; ++i)
            {
                layerQueries.Add(new QueryLayerParams(modeState.Arguments.Layers[i].ResourceGroup,
                    modeState.Arguments.Layers[i].Layer,
                    new Dictionary<string, object>
                    {
                        { Constants.AdditionalFilter, filters.ElementAtOrDefault(i) }
                    }));
            }

            var layerList = await _layerQuerier.GetLayersAsync(layerQueries, traceIdentifier);
            if (layerList.Count == 0)
            {
                return (null, null, new MemoryStream());
            }

            var viewPort = new Viewport
            {
                Extent = modeState.Arguments.Envelope,
                Width = width,
                Height = height,
                Transparent = transparent,
                Bordered = extras.TryGetValue("Bordered", out var b) && (bool)b
            };

            var scale = GeographicUtilities.CalculateOGCScale(modeState.Arguments.Envelope, modeState.Arguments.SRID,
                width, dpi);
            var map = new Map();
            map.SetId(traceIdentifier)
                .SetSrid(modeState.Arguments.SRID)
                .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                .SetLogger(_logger)
                .SetGraphicsContextFactory(_graphicsServiceProvider)
                .AddLayers(layerList);
            var image = await map.GetImageAsync(viewPort, format);

            return (null, null, image);
            // return MapResult.Ok(image, format);
        }
        catch (Exception e)
        {
            _logger.LogError("{Url}, {Exception}", displayUrl, e.ToString());
            return ("InternalError", e.Message, null);
        }
    }

    public async Task<(string Code, string Message, FeatureCollection Features)> GetFeatureInfoAsync(string layers,
        string infoFormat, int featureCount, string srs, string bbox, int width,
        int height, double x, double y, IDictionary<string, object> arguments)
    {
        var traceIdentifier = arguments.GetTraceIdentifier();
        var displayUrl =
            $"[{traceIdentifier}] LAYERS={layers}&FORMAT={infoFormat}&SRS={srs}&BBOX={bbox}&WIDTH={width}&HEIGHT={height}";
        try
        {
            var modeState =
                ArgumentsValidator.VerifyAndBuildWmsGetFeatureInfoArguments(layers, srs, bbox, width, height, x, y,
                    featureCount);
            if (!string.IsNullOrEmpty(modeState.Code))
            {
                _logger.LogError("{Url}, arguments error", displayUrl);
                return (modeState.Code, modeState.Message, null);
            }

            var layerQueries =
                new List<QueryLayerParams>();

            for (var i = 0; i < modeState.Arguments.Layers.Count; ++i)
            {
                layerQueries.Add(
                    new QueryLayerParams(modeState.Arguments.Layers[i].ResourceGroup,
                        modeState.Arguments.Layers[i].Layer));
            }

            var layerList = await _layerQuerier.GetLayersAsync(layerQueries, traceIdentifier);

            var featureCollection = await
                GetFeatureInfoAsync(layerList, featureCount, srs, modeState.Arguments.Envelope, width, height, x, y);

            if (EnvironmentVariables.EnableSensitiveDataLogging)
            {
                _logger.LogInformation("Query features {Url}, target layers: {Layers}, count: {Count}", displayUrl,
                    string.Join(", ", layerList.Select(z => z.Name)), featureCollection.Count);
            }

            return (null, null, featureCollection);
        }
        catch (Exception e)
        {
            _logger.LogError("{Url}, {Exception}", displayUrl, e.ToString());
            return ("InternalError", e.Message, null);
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

        var latLon = GeographicUtilities.CalculateLatLongFromGrid(bbox, pixelWidth, pixelHeight, (int)x, (int)y);

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
            var targetEnvelope = envelope.Transform(srid, spatialDatabaseSource.Srid);
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
}