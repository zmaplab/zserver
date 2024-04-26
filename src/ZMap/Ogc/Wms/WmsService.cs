using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using ZMap.Extensions;
using ZMap.Infrastructure;
using ZMap.Permission;
using ZMap.Source;
using ZMap.Store;

namespace ZMap.Ogc.Wms;

public class WmsService(
    IPermissionService permissionService,
    IGraphicsServiceProvider graphicsServiceProvider,
    ILayerQueryService layerQueryService)
{
    private static readonly ILogger Logger = Log.CreateLogger<WmsService>();
    
    public async ValueTask<MapResult> GetMapAsync(string layers, string styles,
        string srs, string bbox, int width,
        int height, string format,
        bool transparent, string bgColor, int time, string formatOptions, string zFilter,
        IDictionary<string, object> arguments)
    {
        var traceIdentifier = arguments.GetTraceIdentifier();
        string displayUrl = null;
        try
        {
            var validateResult =
                ArgumentsValidator.VerifyAndBuildWmsGetMapArguments(layers, srs, bbox, width, height, format);
            if (!string.IsNullOrEmpty(validateResult.Code))
            {
                displayUrl = GetMapDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, format, formatOptions,
                    zFilter);
                Logger.LogError("{Url}, arguments error", displayUrl);
                return new MapResult(Stream.Null, validateResult.Code, validateResult.Message);
            }

            var requestArguments = validateResult.Arguments;

            var dpi = Utility.GetDpi(formatOptions);

            var filterList = string.IsNullOrWhiteSpace(zFilter)
                ? []
                : zFilter.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var styleList = string.IsNullOrWhiteSpace(styles)
                ? []
                : styles.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var layerQueries =
                new List<LayerQuery>();

            for (var i = 0; i < requestArguments.Layers.Count; ++i)
            {
                var layer = validateResult.Arguments.Layers.ElementAt(i);
                layerQueries.Add(new LayerQuery(layer.ResourceGroup,
                    layer.Layer, styleList.ElementAtOrDefault(i),
                    new Dictionary<string, object>
                    {
                        { Defaults.AdditionalFilter, filterList.ElementAtOrDefault(i) }
                    }));
            }

            var layerList = await layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);
            if (layerList.Count == 0)
            {
                return new MapResult(Stream.Null, null, null);
            }

            foreach (var layer in layerList)
            {
                var permission =
                    await permissionService.EnforceAsync("read", layer.GetResourceId(), PolicyEffect.Allow);
                if (permission)
                {
                    continue;
                }

                return new MapResult(Stream.Null, "403", "Forbidden");
            }

            var viewPort = new Viewport
            {
                Extent = validateResult.Arguments.Envelope,
                Width = width,
                Height = height,
                Transparent = transparent,
                Bordered = arguments.TryGetValue("Bordered", out var b) && (bool)b
            };

            var scale = GeographicUtility.CalculateOGCScale(validateResult.Arguments.Envelope,
                validateResult.Arguments.SRID,
                width, dpi);
            var map = new Map();
            map.SetId(traceIdentifier)
                .SetSrid(validateResult.Arguments.SRID)
                .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                .SetGraphicsContextFactory(graphicsServiceProvider)
                .AddLayers(layerList);
            var image = await map.GetImageAsync(viewPort, format);
            return new MapResult(image, null, null);
        }
        catch (Exception e)
        {
            displayUrl ??= GetMapDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, format, formatOptions,
                zFilter);
            Logger.LogError(e, "请求 {Url} 失败", displayUrl);
            return new MapResult(Stream.Null, "InternalError", e.Message);
        }
    }

    public async ValueTask<GetFeatureInfoResult> GetFeatureInfoAsync(string layers,
        string srs, string bbox, int width, int height,
        string infoFormat, int featureCount,
        double x, double y, IDictionary<string, object> arguments)
    {
        var traceIdentifier = arguments.GetTraceIdentifier();
        string displayUrl = null;
        try
        {
            var validateResult =
                ArgumentsValidator.VerifyAndBuildWmsGetFeatureInfoArguments(layers, srs, bbox, width, height, x, y,
                    featureCount);
            if (!string.IsNullOrEmpty(validateResult.Code))
            {
                displayUrl = GetFeatureInfoDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, infoFormat);
                Logger.LogError("{Url}, arguments error", displayUrl);
                return new GetFeatureInfoResult(null, validateResult.Code, validateResult.Message);
            }

            var layerQueries =
                new List<LayerQuery>();

            for (var i = 0; i < validateResult.Arguments.Layers.Count; ++i)
            {
                var layer = validateResult.Arguments.Layers.ElementAt(i);
                layerQueries.Add(new LayerQuery(layer.ResourceGroup,
                    layer.Layer, null,
                    new Dictionary<string, object>()));
            }

            var layerList = await layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);

            var featureCollection = await
                GetFeatureInfoAsync(layerList, featureCount, srs, validateResult.Arguments.Envelope, width, height, x,
                    y);

            if (!EnvironmentVariables.EnableSensitiveDataLogging)
            {
                return new GetFeatureInfoResult(featureCollection, null, null);
            }

            displayUrl = GetFeatureInfoDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, infoFormat);
            Logger.LogInformation("GetFeatureInfo {Url}, hit layers: {Layers}, count: {Count}", displayUrl,
                string.Join(", ", layerList.Select(z => z.Name)), featureCollection.Count);

            return new GetFeatureInfoResult(featureCollection, null, null);
        }
        catch (Exception e)
        {
            displayUrl ??= GetFeatureInfoDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, infoFormat);
            Logger.LogError(e, "请求 {Url} 失败", displayUrl);
            return new GetFeatureInfoResult(null, "InternalError", e.Message);
        }
    }

    private async Task<FeatureCollection> GetFeatureInfoAsync(List<Layer> layers,
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

        var latLon = GeographicUtility.CalculateLatLongFromGrid(bbox, pixelWidth, pixelHeight, (int)x, (int)y);

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

    private string GetMapDisplayUrl(string traceIdentifier, string layers,
        string srs, string bbox, int width,
        int height, string format,
        string formatOptions, string cqlFilter)
    {
        return
            $"[{traceIdentifier}] LAYERS={layers}&FORMAT={format}&SRS={srs}&BBOX={bbox}&WIDTH={width}&HEIGHT={height}&FILTER={cqlFilter}&FORMAT_OPTIONS={formatOptions}";
    }

    private string GetFeatureInfoDisplayUrl(string traceIdentifier, string layers,
        string srs, string bbox, int width,
        int height, string infoFormat)
    {
        return
            $"[{traceIdentifier}] LAYERS={layers}&FORMAT={infoFormat}&SRS={srs}&BBOX={bbox}&WIDTH={width}&HEIGHT={height}";
    }
}