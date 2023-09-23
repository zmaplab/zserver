using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZMap.Extensions;
using ZMap.Infrastructure;
using ZMap.Store;

namespace ZMap.Ogc.Wmts;

public class WmtsService
{
    private readonly ILogger<WmtsService> _logger;
    private readonly IGraphicsServiceProvider _graphicsServiceProvider;
    private readonly ILayerQueryService _layerQueryService;
    private readonly IGridSetStore _gridSetStore;

    public WmtsService(ILogger<WmtsService> logger, IGraphicsServiceProvider graphicsServiceProvider,
        ILayerQueryService layerQueryService, IGridSetStore gridSetStore)
    {
        _logger = logger;
        _graphicsServiceProvider = graphicsServiceProvider;
        _layerQueryService = layerQueryService;
        _gridSetStore = gridSetStore;
    }

    public async ValueTask<MapResult> GetTileAsync(string layers, string styles,
        string format,
        string tileMatrixSet, string tileMatrix, int tileRow,
        int tileCol, string cqlFilter, IDictionary<string, object> arguments)
    {
        var traceIdentifier = arguments.GetTraceIdentifier();
        var displayUrl =
            $"[{traceIdentifier}] LAYERS={layers}&STYLES={styles}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";

        try
        {
            if (string.IsNullOrWhiteSpace(layers))
            {
                _logger.LogError("{Url}, no layers have been requested", displayUrl);
                return new MapResult(Stream.Null, "LayerNotDefined", "No layers have been requested");
            }

            if (string.IsNullOrWhiteSpace(tileMatrixSet))
            {
                _logger.LogError("{Url}, no tile matrix set requested", displayUrl);
                return new MapResult(Stream.Null, "InvalidTileMatrixSet", "No tile matrix set requested");
            }

            var gridSet = await _gridSetStore.FindAsync(tileMatrixSet);

            if (gridSet == null)
            {
                _logger.LogError("{Url}, could not find tile matrix set", displayUrl);
                return new MapResult(Stream.Null, "TileMatrixSetNotDefined",
                    $"Could not find tile matrix set {tileMatrixSet}");
            }

            var path = Utilities.GetWmtsKey(layers, cqlFilter, format, tileMatrixSet, tileRow.ToString(),
                tileCol.ToString());

            var folder = Path.GetDirectoryName(path);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(path))
            {
                _logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", traceIdentifier, displayUrl);
                return new MapResult(File.OpenRead(path), null, null);
            }

            var tuple = gridSet.GetEnvelope(tileMatrix, tileCol, tileRow);
            if (tuple == default)
            {
                _logger.LogError("{Url}, could not get envelope from grid set", displayUrl);
                return new MapResult(Stream.Null, null, null);
            }

            // 如果有多个图层过滤条件
            var filters = string.IsNullOrWhiteSpace(cqlFilter)
                ? Array.Empty<string>()
                : cqlFilter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var layerQueries =
                new List<QueryLayerParams>();

            var layerNames = layers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < layerNames.Length; i++)
            {
                var layerName = layerNames[i];
                var layerQuery = layerName.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                switch (layerQuery.Length)
                {
                    case 2:
                        layerQueries.Add(new QueryLayerParams(layerQuery[0], layerQuery[1],
                            new Dictionary<string, object>
                            {
                                { Defaults.AdditionalFilter, filters.ElementAtOrDefault(i) }
                            }));
                        break;
                    case 1:
                        layerQueries.Add(new QueryLayerParams(null, layerQuery[0], new Dictionary<string, object>
                        {
                            { Defaults.AdditionalFilter, filters.ElementAtOrDefault(i) }
                        }));
                        break;
                    default:
                    {
                        _logger.LogError("{Url}, layer format is incorrect {Layer}", displayUrl, layerName);
                        return new MapResult(Stream.Null, "LayerFormatIncorrect",
                            $"layer format is incorrect {layerName}");
                    }
                }
            }

            var layerList = await _layerQueryService.GetLayersAsync(layerQueries, traceIdentifier);
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
                .SetLogger(_logger)
                .SetGraphicsContextFactory(_graphicsServiceProvider)
                .AddLayers(layerList);
            var image = await map.GetImageAsync(viewPort, format);
            await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            await image.CopyToAsync(fileStream);
            image.Seek(0, SeekOrigin.Begin);
            return new MapResult(image, null, null);
        }
        catch (Exception e)
        {
            _logger.LogError("{Url}, {Exception}", displayUrl, e.ToString());
            return new MapResult(Stream.Null, "InternalError", e.Message);
        }
    }
}