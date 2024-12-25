using System.Net.Http;

namespace ZMap.Ogc.Wms;

public class WmsService(
    IPermissionService permissionService,
    IGraphicsServiceProvider graphicsServiceProvider,
    IHttpClientFactory httpClientFactory,
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
                ParameterValidator.VerifyAndBuildWmsGetMapArguments(layers, srs, bbox, width, height, format, styles,
                    zFilter);
            if (!string.IsNullOrEmpty(validateResult.Code))
            {
                displayUrl = GetMapDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, format, formatOptions,
                    zFilter);
                Logger.LogError("{Url}, arguments error: {Code}, message: {Message}", displayUrl, validateResult.Code,
                    validateResult.Message);
                return new MapResult(Stream.Null, validateResult.Code, validateResult.Message);
            }

            var dpi = Utility.GetDpi(formatOptions);
            var layerQueries = new List<LayerQuery>();
            var requestParameters = validateResult.Parameters;

            for (var i = 0; i < requestParameters.Layers.Count; ++i)
            {
                var layer = requestParameters.Layers.ElementAt(i);
                layerQueries.Add(new LayerQuery(layer.ResourceGroup, layer.Layer,
                    requestParameters.Styles.ElementAtOrDefault(i)));
            }

            var tuple = await layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);
            var layerList = tuple.Layers;
            if (tuple.FetchCount == 0 || layerList.Count == 0 || layerList.Count != layerQueries.Count)
            {
                Logger.LogError("[{TraceIdentifier}] 图层 {Layer} 中存在缺失图层", traceIdentifier, layers);
                return new MapResult(Stream.Null, "QueryLayerError", null);
            }

            for (var i = 0; i < requestParameters.Layers.Count; ++i)
            {
                var layer = layerList[i];
                layer.HttpClientFactory = httpClientFactory;
                layer.Filter = requestParameters.Filters.ElementAtOrDefault(i);

                var permission =
                    await permissionService.EnforceAsync("read", layer.ResourceId, PolicyEffect.Allow);
                if (permission)
                {
                    continue;
                }

                return new MapResult(Stream.Null, "403", "Forbidden");
            }

            var viewport = new Viewport
            {
                Extent = validateResult.Parameters.Envelope,
                Width = width,
                Height = height,
                Transparent = transparent,
                BackgroundColor = bgColor,
                Bordered = arguments.TryGetValue("Bordered", out var b) && (bool)b
            };

            var scale = GeographicUtility.CalculateOGCScale(validateResult.Parameters.Envelope,
                validateResult.Parameters.SRID,
                width, dpi);
            var map = new Map();
            await map.SetId(traceIdentifier)
                .SetSrid(validateResult.Parameters.SRID)
                .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                .SetGraphicsContextFactory(graphicsServiceProvider)
                .AddLayers(layerList);
            var image = await map.GetImageAsync(viewport, format);
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
                ParameterValidator.VerifyAndBuildWmsGetFeatureInfoArguments(layers, srs, bbox, width, height, x, y,
                    featureCount);
            if (!string.IsNullOrEmpty(validateResult.Code))
            {
                displayUrl = GetFeatureInfoDisplayUrl(traceIdentifier, layers, srs, bbox, width, height, infoFormat);
                Logger.LogError("{Url}, arguments error", displayUrl);
                return new GetFeatureInfoResult(null, validateResult.Code, validateResult.Message);
            }

            var layerQueries =
                new List<LayerQuery>();

            for (var i = 0; i < validateResult.Parameters.Layers.Count; ++i)
            {
                var layer = validateResult.Parameters.Layers.ElementAt(i);
                layerQueries.Add(new LayerQuery(layer.ResourceGroup, layer.Layer, null));
            }

            var tuple = await layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);
            var layerList = tuple.Layers;
            var featureCollection = await GetFeatureInfoAsync(layerList, featureCount, srs,
                validateResult.Parameters.Envelope,
                width, height, x, y);

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
                .GetFeaturesAsync(targetEnvelope, null);

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