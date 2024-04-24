using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZMap.Extensions;
using ZMap.Infrastructure;
using ZMap.Permission;
using ZMap.Store;

namespace ZMap.Ogc.Wmts;

public class WmtsService(
    ILogger<WmtsService> logger,
    IGraphicsServiceProvider graphicsServiceProvider,
    ILayerQueryService layerQueryService,
    IPermissionService permissionService,
    IGridSetStore gridSetStore)
{
    public async ValueTask<MapResult> GetTileAsync(string layers, string styles,
        string format,
        string tileMatrixSet, string tileMatrix, int tileRow,
        int tileCol, string cqlFilter, IDictionary<string, object> arguments)
    {
        var traceIdentifier = arguments.GetTraceIdentifier();
        string displayUrl = null;

        try
        {
            if (string.IsNullOrWhiteSpace(layers))
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, cqlFilter);
                logger.LogError("{Url}, no layers have been requested", displayUrl);
                return new MapResult(Stream.Null, "LayerNotDefined", "No layers have been requested");
            }

            if (string.IsNullOrWhiteSpace(tileMatrixSet))
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, cqlFilter);
                logger.LogError("{Url}, no tile matrix set requested", displayUrl);
                return new MapResult(Stream.Null, "InvalidTileMatrixSet", "No tile matrix set requested");
            }

            var gridSet = await gridSetStore.FindAsync(tileMatrixSet);

            if (gridSet == null)
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, cqlFilter);
                logger.LogError("{Url}, could not find tile matrix set", displayUrl);
                return new MapResult(Stream.Null, "TileMatrixSetNotDefined",
                    $"Could not find tile matrix set {tileMatrixSet}");
            }

            var path = Utility.GetWmtsPath(layers, cqlFilter, format, tileMatrixSet, tileRow, tileCol);
            if (string.IsNullOrEmpty(path))
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, cqlFilter);
                logger.LogError("{Url}, wmts key is empty", displayUrl);
                return new MapResult(Stream.Null, "WMTSKeyIsEmpty",
                    "wmts key is empty");
            }

            if (File.Exists(path))
            {
                if (EnvironmentVariables.EnableSensitiveDataLogging)
                {
                    displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                        tileRow, tileCol, cqlFilter);
                    logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", traceIdentifier, displayUrl);
                }

                return new MapResult(File.OpenRead(path), null, null);
            }

            var folder = Path.GetDirectoryName(path);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var tuple = gridSet.GetEnvelope(tileMatrix, tileCol, tileRow);
            if (tuple == default)
            {
                displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                    tileRow, tileCol, cqlFilter);
                logger.LogError("{Url}, could not get envelope from grid set", displayUrl);
                return new MapResult(Stream.Null, null, null);
            }

            // 如果有多个图层过滤条件
            var filterList = string.IsNullOrWhiteSpace(cqlFilter)
                ? []
                : cqlFilter.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var layerQueries =
                new List<LayerQuery>();

            var layerNames = layers.Split(',', StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < layerNames.Length; i++)
            {
                var layerName = layerNames[i];
                var layerQuery = layerName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var filter = filterList.ElementAtOrDefault(i);
                switch (layerQuery.Length)
                {
                    case 2:
                        layerQueries.Add(new LayerQuery(layerQuery[0], layerQuery[1],
                            new Dictionary<string, object>
                            {
                                { Defaults.AdditionalFilter, filter }
                            }));
                        break;
                    case 1:
                        layerQueries.Add(new LayerQuery(null, layerQuery[0], new Dictionary<string, object>
                        {
                            { Defaults.AdditionalFilter, filter }
                        }));
                        break;
                    default:
                    {
                        displayUrl = GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet,
                            tileMatrix,
                            tileRow, tileCol, cqlFilter);
                        logger.LogError("{Url}, layer format is incorrect {Layer}", displayUrl, layerName);
                        return new MapResult(Stream.Null, "LayerFormatIncorrect",
                            $"layer format is incorrect {layerName}");
                    }
                }
            }

            var layerList = await layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);
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

            var scale = tuple.ScaleDenominator;
            var viewPort = new Viewport
            {
                Extent = tuple.Extent,
                Width = gridSet.TileWidth,
                Height = gridSet.TileHeight,
                Transparent = true,
                Bordered = arguments.TryGetValue("Bordered", out var b) && (bool)b
            };

            var map = new Map();
            map.SetId(traceIdentifier)
                .SetSrid(gridSet.SRID)
                .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                .SetLogger(logger)
                .SetGraphicsContextFactory(graphicsServiceProvider)
                .AddLayers(layerList);
            var image = await map.GetImageAsync(viewPort, format);
            await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            await image.CopyToAsync(fileStream);
            image.Seek(0, SeekOrigin.Begin);
            return new MapResult(image, null, null);
        }
        catch (Exception e)
        {
            displayUrl ??= GetTileDisplayUrl(traceIdentifier, layers, styles, format, tileMatrixSet, tileMatrix,
                tileRow, tileCol, cqlFilter);
            logger.LogError(e, "请求 {Url} 失败", displayUrl);
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