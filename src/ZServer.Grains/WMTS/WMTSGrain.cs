using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using ZMap;
using ZMap.Infrastructure;
using ZServer.Extensions;
using ZServer.Interfaces;
using ZServer.Interfaces.WMTS;
using ZServer.Store;

namespace ZServer.Grains.WMTS
{
    // ReSharper disable once InconsistentNaming
    public class WMTSGrain : Grain, IWMTSGrain
    {
        private readonly ILogger<WMTSGrain> _logger;
        private readonly ILayerQuerier _layerQuerier;
        private readonly IGridSetStore _gridSetStore;
        private readonly IGraphicsServiceProvider _graphicsServiceProvider;

        public WMTSGrain(ILogger<WMTSGrain> logger,
            IGridSetStore gridSetStore,
            IGraphicsServiceProvider graphicsServiceProvider,
            ILayerQuerier layerQuerier)
        {
            _logger = logger;
            _gridSetStore = gridSetStore;
            _graphicsServiceProvider = graphicsServiceProvider;
            _layerQuerier = layerQuerier;
        }

        public async Task<MapResult> GetTileAsync(string layers, string styles, string format,
            string tileMatrixSet,
            string tileMatrix, int tileRow,
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
                    return MapResult.Failed("No layers have been requested", "LayerNotDefined");
                }

                if (string.IsNullOrWhiteSpace(tileMatrixSet))
                {
                    _logger.LogError("{Url}, no tile matrix set requested", displayUrl);
                    return MapResult.Failed("No tile matrix set requested", "InvalidTileMatrixSet");
                }

                var gridSet = await _gridSetStore.FindAsync(tileMatrixSet);

                if (gridSet == null)
                {
                    _logger.LogError("{Url}, could not find tile matrix set", displayUrl);
                    return MapResult.Failed($"Could not find tile matrix set {tileMatrixSet}",
                        "TileMatrixSetNotDefined");
                }

                var layerKey = MurmurHashAlgorithmService.ComputeHash(Encoding.UTF8.GetBytes(layers));

                var cqlFilterHash = string.IsNullOrWhiteSpace(cqlFilter)
                    ? string.Empty
                    : MurmurHashAlgorithmService.ComputeHash(Encoding.UTF8.GetBytes(cqlFilter));

                // todo: 设计 cache 接口， 方便扩展 OSS 或者别的 分布式文件系统
                var path = Path.Combine(AppContext.BaseDirectory,
                    $"cache/wmts/{layerKey}/{tileMatrix}/{tileRow}/{tileCol}_{cqlFilterHash}{Utilities.GetImageExtension(format)}");
                var folder = Path.GetDirectoryName(path);
                if (folder != null && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

#if !DEBUG
                if (File.Exists(path))
                {
                    _logger.LogInformation($"[{traceIdentifier}] {displayUrl}, CACHED");
                    return MapResult.Ok(await File.ReadAllBytesAsync(path), format);
                }
#endif
                format = string.IsNullOrWhiteSpace(format) ? "image/png" : format;

                var tuple = gridSet.GetEnvelope(tileMatrix, tileCol, tileRow);
                if (tuple == default)
                {
                    _logger.LogError("{Url}, could not get envelope from grid set", displayUrl);
                    return MapResult.EmptyMap(format);
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
                                    { Constants.AdditionalFilter, filters.ElementAtOrDefault(i) }
                                }));
                            break;
                        case 1:
                            layerQueries.Add(new QueryLayerParams(null, layerQuery[0], new Dictionary<string, object>
                            {
                                { Constants.AdditionalFilter, filters.ElementAtOrDefault(i) }
                            }));
                            break;
                        default:
                        {
                            _logger.LogError("{Url}, layer format is incorrect {Layer}", displayUrl, layerName);
                            return MapResult.Failed($"layer format is incorrect {layerName}",
                                "LayerFormatIncorrect");
                        }
                    }
                }

                var layerList = await _layerQuerier.GetLayersAsync(layerQueries, traceIdentifier);
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

                await File.WriteAllBytesAsync(path, image);
                return MapResult.Ok(image, format);
            }
            catch (Exception e)
            {
                _logger.LogError("{Url}, {Exception}", displayUrl, e.ToString());
                return MapResult.Failed(e.Message, "InternalError");
            }
        }
    }
}