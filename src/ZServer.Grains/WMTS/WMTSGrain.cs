using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using ZMap;
using ZMap.Utilities;
using ZServer.Extensions;
using ZServer.HashAlgorithm;
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
        private readonly IHashAlgorithmService _hashAlgorithmService;
        private readonly IGraphicsServiceProvider _graphicsServiceProvider;

        public WMTSGrain(ILogger<WMTSGrain> logger,
            IGridSetStore gridSetStore, IHashAlgorithmService hashAlgorithmService,
            IGraphicsServiceProvider graphicsServiceProvider,
            ILayerQuerier layerQuerier)
        {
            _logger = logger;
            _gridSetStore = gridSetStore;
            _hashAlgorithmService = hashAlgorithmService;
            _graphicsServiceProvider = graphicsServiceProvider;
            _layerQuerier = layerQuerier;
        }

        public async Task<MapResult> GetTileAsync(string layers, string styles, string format,
            string tileMatrixSet,
            string tileMatrix, int tileRow,
            int tileCol, IDictionary<string, object> arguments)
        {
            var traceIdentifier = arguments.GetTraceIdentifier();
            var displayUrl =
                $"[{traceIdentifier}] LAYERS={layers}&STYLES={styles}&FORMAT={format}&TILEMATRIXSET={tileMatrixSet}&TILEMATRIX={tileMatrix}&TILEROW={tileRow}&TILECOL={tileCol}";

            try
            {
                if (string.IsNullOrWhiteSpace(layers))
                {
                    var msg = $"{displayUrl}, no layers have been requested";
                    _logger.LogWarning(msg);
                    return MapResult.Failed("No layers have been requested", "LayerNotDefined");
                }

                if (string.IsNullOrWhiteSpace(format))
                {
                    var msg = $"{displayUrl}, no output map format requested";
                    _logger.LogError(msg);
                    return MapResult.Failed("No output map format requested", "InvalidFormat");
                }

                if (string.IsNullOrWhiteSpace(tileMatrixSet))
                {
                    var msg = $"{displayUrl}, no tile matrix set requested";
                    _logger.LogError(msg);
                    return MapResult.Failed("No tile matrix set requested", "InvalidTileMatrixSet");
                }

                var gridSet = await _gridSetStore.FindAsync(tileMatrixSet);

                if (gridSet == null)
                {
                    var msg = $"{displayUrl}, could not find tile matrix set";
                    _logger.LogError(msg);
                    return MapResult.Failed($"Could not find tile matrix set {tileMatrixSet}",
                        "TileMatrixSetNotDefined");
                }

                var layerKey = BitConverter.ToString(_hashAlgorithmService.ComputeHash(Encoding.UTF8.GetBytes(layers)))
                    .Replace("-", string.Empty);

                // todo: 设计 cache 接口， 方便扩展 OSS 或者别的 分布式文件系统
                var path = Path.Combine(AppContext.BaseDirectory,
                    $"cache/wmts/{layerKey}/{tileMatrix}/{tileRow}/{tileCol}{ImageFormatUtilities.GetExtension(format)}");
                var folder = Path.GetDirectoryName(path);
                if (folder != null && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

#if !DEBUG
                if (File.Exists(path))
                {
                    _logger.LogInformation($"[{traceIdentifier}] {displayUrl}, CACHED");
                    return MapResult.Ok(File.ReadAllBytes(path), format);
                }
#endif

                var tuple = gridSet.GetEnvelope(tileMatrix, tileCol, tileRow);
                if (tuple == default)
                {
                    return MapResult.EmptyMap(format);
                }

                var layerQueries =
                    new List<QueryLayerParams>();
                foreach (var layer in layers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var layerQuery = layer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    switch (layerQuery.Length)
                    {
                        case 2:
                            layerQueries.Add(new QueryLayerParams(layerQuery[0], layerQuery[1],
                                new Dictionary<string, object>
                                {
                                    { Constants.AdditionalFilter, string.Empty }
                                }));
                            break;
                        case 1:
                            layerQueries.Add(new QueryLayerParams(null, layerQuery[0], new Dictionary<string, object>
                            {
                                { Constants.AdditionalFilter, string.Empty }
                            }));
                            break;
                        default:
                        {
                            var msg = $"{displayUrl}, layer format is incorrect {layer}";
                            _logger.LogError(msg);
                            return MapResult.Failed($"Could not find layer {layer}",
                                "LayerNotDefined");
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
                    Bordered = false
                };

                var map = new Map();
                map.SetId(traceIdentifier)
                    .SetSRID(gridSet.SRID)
                    .SetZoom(new Zoom(scale, ZoomUnits.Scale))
                    .SetLogger(_logger)
                    .SetGraphicsContextFactory(_graphicsServiceProvider)
                    .AddLayers(layerList);
                var image = await map.GetImageAsync(viewPort, format);

                File.WriteAllBytes(path, image);
                return MapResult.Ok(image, format);
            }
            catch (Exception e)
            {
                var msg = $"{displayUrl}, {e}";
                _logger.LogError(msg);
                return MapResult.Failed(e.Message, "InternalError");
            }
        }
    }
}