using System.Net.Http;

namespace ZMap.Ogc.Wmts;

public class WmtsService(
    IGraphicsServiceProvider graphicsServiceProvider,
    ILayerQueryService layerQueryService,
    IPermissionService permissionService,
    IHttpClientFactory httpClientFactory,
    IGridSetStore gridSetStore)
{
    private static readonly ILogger Logger = Log.CreateLogger<WmtsService>();

    public async ValueTask<MapResult> GetTileAsync(string layers, string styles,
        string format,
        string tileMatrixSet, string tileMatrix, int tileRow,
        int tileCol, string zFilter, IDictionary<string, object> arguments)
    {
        var traceIdentifier = arguments.GetTraceIdentifier();
        string displayUrl = null;

        try
        {
            if (string.IsNullOrEmpty(layers))
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, zFilter);
                Logger.LogError("{Url}, no layers have been requested", displayUrl);
                return new MapResult(Stream.Null, "LayerNotDefined", "No layers have been requested");
            }

            if (string.IsNullOrEmpty(tileMatrixSet))
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, zFilter);
                Logger.LogError("{Url}, no tile matrix set requested", displayUrl);
                return new MapResult(Stream.Null, "InvalidTileMatrixSet", "No tile matrix set requested");
            }

            var gridSet = await gridSetStore.FindAsync(tileMatrixSet);

            if (gridSet == null)
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, zFilter);
                Logger.LogError("{Url}, could not find tile matrix set", displayUrl);
                return new MapResult(Stream.Null, "TileMatrixSetNotDefined",
                    $"Could not find tile matrix set {tileMatrixSet}");
            }

            var tuple = Utility.GetWmtsPath(layers, zFilter, format, tileMatrixSet, tileMatrix, tileRow, tileCol);
            if (string.IsNullOrEmpty(tuple.FullPath))
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, zFilter);
                Logger.LogError("{Url}, wmts key is empty", displayUrl);
                return new MapResult(Stream.Null, "WMTSKeyIsEmpty",
                    "wmts key is empty");
            }

#if !DEBUG
            if (File.Exists(tuple.FullPath))
            {
                if (EnvironmentVariables.EnableSensitiveDataLogging)
                {
                    displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                        tileRow, tileCol, zFilter);
                    Logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", traceIdentifier, displayUrl);
                }

                return new MapResult(File.OpenRead(tuple.FullPath), null, null);
            }
#endif

            var folder = Path.GetDirectoryName(tuple.FullPath);
            // TODO: 优化减小磁盘 IO
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var gridSetEnvelope = gridSet.GetEnvelope(tileMatrix, tileCol, tileRow);
            if (gridSetEnvelope == default)
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, zFilter);
                Logger.LogError("{Url}, could not get envelope from grid set", displayUrl);
                return new MapResult(Stream.Null, null, null);
            }

            var layerQueries =
                new List<LayerQuery>();

            var layerNames = layers.Split(',', StringSplitOptions.RemoveEmptyEntries);
            // 如果有多个图层过滤条件
            var filterList = string.IsNullOrEmpty(zFilter)
                ? []
                : zFilter.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (filterList.Length > 0 && filterList.Length != layerNames.Length)
            {
                return new MapResult(Stream.Null, "FilterDefinedError", "filter count not match layer count");
            }

            var styleList = string.IsNullOrEmpty(styles)
                ? []
                : styles.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (styleList.Length > 0 && styleList.Length != layerNames.Length)
            {
                return new MapResult(Stream.Null, "StyleDefinedError", "style count not match layer count");
            }

            for (var i = 0; i < layerNames.Length; i++)
            {
                var layerName = layerNames[i];
                var layerQuery = layerName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                // var filter = filterList.ElementAtOrDefault(i);
                switch (layerQuery.Length)
                {
                    case 2:
                        layerQueries.Add(new LayerQuery(layerQuery[0], layerQuery[1], styleList.ElementAtOrDefault(i)));
                        break;
                    case 1:
                        layerQueries.Add(new LayerQuery(null, layerQuery[0], styleList.ElementAtOrDefault(i)));
                        break;
                    default:
                    {
                        displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet,
                            tileMatrix,
                            tileRow, tileCol, zFilter);
                        Logger.LogError("{Url}, layer format is incorrect {Layer}", displayUrl, layerName);
                        return new MapResult(Stream.Null, "LayerFormatIncorrect",
                            $"layer format is incorrect {layerName}");
                    }
                }
            }

            var layerTuple = await layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);
            var layerList = layerTuple.Layers;
            if (layerTuple.FetchCount == 0 || layerList.Count == 0 || layerList.Count != layerQueries.Count)
            {
                Logger.LogError("[{TraceIdentifier}] 图层 {Layer} 中存在缺失图层", traceIdentifier, layers);
                return new MapResult(Stream.Null, "QueryLayerError", null);
            }

            for (var i = 0; i < layerList.Count; ++i)
            {
                var layer = layerList[i];
                layer.HttpClientFactory = httpClientFactory;
                layer.Filter = filterList.ElementAtOrDefault(i);

                var permission =
                    await permissionService.EnforceAsync("read", layer.ResourceId, PolicyEffect.Allow);
                if (permission)
                {
                    continue;
                }

                return new MapResult(Stream.Null, "403", "Forbidden");
            }

            var scale = gridSetEnvelope.ScaleDenominator;
            var bordered = arguments.TryGetValue("Bordered", out var b) && (bool)b;
            var viewPort = new Viewport
            {
                Extent = gridSetEnvelope.Extent,
                Width = gridSet.TileWidth,
                Height = gridSet.TileHeight,
                Transparent = true,
                Bordered = bordered
            };

            var map = new Map();
            await map.SetId(traceIdentifier)
                .SetSrid(gridSet.SRID)
                .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                .SetGraphicsContextFactory(graphicsServiceProvider)
                .AddLayers(layerList);
            var image = await map.GetImageAsync(viewPort, format);
            await using var fileStream = new FileStream(tuple.FullPath, FileMode.Create, FileAccess.Write);
            await image.CopyToAsync(fileStream);
            image.Seek(0, SeekOrigin.Begin);
            return new MapResult(image, null, null);
        }
        catch (Exception e)
        {
            displayUrl ??= GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                tileRow, tileCol, zFilter);
            Logger.LogError(e, "请求 {Url} 失败", displayUrl);
            return new MapResult(Stream.Null, "InternalError", e.Message);
        }
    }

    private string GetTileDisplayUrl(string traceIdentifier, string layers, string styles, string format,
        string tileMatrixSet, string tileMatrix, int tileRow, int tileCol, string filter)
    {
        return
            $"[{traceIdentifier}] LAYERS={layers}&STYLES={styles}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}&CQL_FILTER={filter}";
    }
}