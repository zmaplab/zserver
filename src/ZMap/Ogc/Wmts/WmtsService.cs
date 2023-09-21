using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
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

    public async Task<(string Code, string Message, Stream Stream)> GetTileAsync(string layers, string styles,
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
                return ("LayerNotDefined", "No layers have been requested", null);
            }

            if (string.IsNullOrWhiteSpace(tileMatrixSet))
            {
                _logger.LogError("{Url}, no tile matrix set requested", displayUrl);
                return ("InvalidTileMatrixSet", "No tile matrix set requested", null);
            }

            var gridSet = await _gridSetStore.FindAsync(tileMatrixSet);

            if (gridSet == null)
            {
                _logger.LogError("{Url}, could not find tile matrix set", displayUrl);
                return ("TileMatrixSetNotDefined", $"Could not find tile matrix set {tileMatrixSet}", null);
            }

            var layerKey = MurmurHashAlgorithmUtilities.ComputeHash(Encoding.UTF8.GetBytes(layers));

            var cqlFilterHash = string.IsNullOrWhiteSpace(cqlFilter)
                ? string.Empty
                : MurmurHashAlgorithmUtilities.ComputeHash(Encoding.UTF8.GetBytes(cqlFilter));

            // todo: 设计 cache 接口， 方便扩展 OSS 或者别的 分布式文件系统
            var path = Path.Combine(AppContext.BaseDirectory,
                string.IsNullOrEmpty(cqlFilterHash)
                    ? $"cache/wmts/{layerKey}/{tileMatrix}/{tileRow}/{tileCol}{Utilities.GetImageExtension(format)}"
                    : $"cache/wmts/{layerKey}/{tileMatrix}/{tileRow}/{tileCol}_{cqlFilterHash}{Utilities.GetImageExtension(format)}");

            var folder = Path.GetDirectoryName(path);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(path))
            {
                _logger.LogInformation("[{TraceIdentifier}] {Url}, CACHED", traceIdentifier, displayUrl);
                return (null, null, File.OpenRead(path));
            }

            var tuple = gridSet.GetEnvelope(tileMatrix, tileCol, tileRow);
            if (tuple == default)
            {
                _logger.LogError("{Url}, could not get envelope from grid set", displayUrl);
                return (null, null, new MemoryStream());
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
                        return ("LayerFormatIncorrect", $"layer format is incorrect {layerName}", null);
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
            return (null, null, image);
        }
        catch (Exception e)
        {
            _logger.LogError("{Url}, {Exception}", displayUrl, e.ToString());
            return ("InternalError", e.Message, null);
        }
    }
}