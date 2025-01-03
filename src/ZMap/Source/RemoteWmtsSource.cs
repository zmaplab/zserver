using System.IO.Compression;
using System.Net.Http;
using ZMap.Ogc.Wmts;
using ZMap.TileGrid;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ZMap.Source;

public partial class RemoteWmtsSource(string url) : ITiledSource, IRemoteHttpSource
{
    private static readonly HttpClient HttpClient = new();
    private string _cacheFolder;
    public string Key { get; set; }
    public string Name { get; set; }
    public int Srid { get; set; }
    public CoordinateSystem CoordinateSystem { get; set; }
    public Envelope Envelope { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string MatrixSet { get; set; }

    public GridSet GridSet { get; set; }

    public IHttpClientFactory HttpClientFactory { get; set; }

    public async Task LoadAsync()
    {
        if (GridSet != null)
        {
            return;
        }

        var tuple = await Cache.GetOrCreateAsync($"{url}_Capabilities", async entry =>
        {
            _cacheFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache/wmts/remote/{Name}");
            if (!Directory.Exists(_cacheFolder))
            {
                Directory.CreateDirectory(_cacheFolder);
            }

            var getCapabilitiesUrl = string.Format(url, 0, 0, 0).Replace("=GetTile", "=GetCapabilities");
            var response = await SendAsync(getCapabilitiesUrl);
            Capabilities capabilities = null;
            if (response.IsSuccessStatusCode)
            {
                string xml;
                // 检查响应是否被 Gzip 压缩
                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    await using var responseStream = await response.Content.ReadAsStreamAsync();
                    await using var decompressionStream =
                        new GZipStream(responseStream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressionStream, Encoding.UTF8);
                    xml = await reader.ReadToEndAsync();
                }
                else
                {
                    // 如果响应未被压缩，直接读取内容
                    xml = await response.Content.ReadAsStringAsync();
                }

                capabilities = Capabilities.LoadFromXml(xml);
            }

            if (capabilities == null)
            {
                throw new ArgumentException("WMTS Capabilities 无法获取");
            }


            var firstTileMatrix = capabilities.Contents.TileMatrixSet.TileMatrices[0];
            var name = capabilities.Contents.TileMatrixSet.Identifier.Trim();
            var match = SridRegex().Match(capabilities.Contents.TileMatrixSet.SupportedCRS);
            if (!match.Success)
            {
                throw new ArgumentException("解析 Capabilities 失败： 空间标识符无法解析");
            }

            var crsNumber = int.Parse(match.Value);
            if (crsNumber == 900913)
            {
                crsNumber = 3857;
            }

            var upperCorner = capabilities.Contents.Layer.BoundingBox.UpperCorner.Split(' ').Select(double.Parse)
                .ToList();
            var lowerCorner = capabilities.Contents.Layer.BoundingBox.LowerCorner.Split(' ').Select(double.Parse)
                .ToList();

            var extent = new Envelope(lowerCorner[0], upperCorner[0], lowerCorner[1], upperCorner[1]);

            var crs = CoordinateReferenceSystem.Get(crsNumber);

            var gridSet = new GridSet
            {
                SRID = crsNumber,
                Name = name,
                TileWidth = firstTileMatrix.TileWidth,
                TileHeight = firstTileMatrix.TileHeight,
                ResolutionsPreserved = false,
                PixelSize = GridSetFactory.DefaultPixelSizeMeter,
                YBaseToggle = false,
                YCoordinateFirst = true
            };

            switch (crs)
            {
                // 地理坐标系（度）
                case GeographicCoordinateSystem:
                    gridSet.MetersPerUnit = GridSetFactory.Epsg4326ToMeters;
                    // TODO:
                    gridSet.Extent = GridSetFactory.GeographicWorld;
                    break;
                // 投影坐标系（米）
                case ProjectedCoordinateSystem:
                    gridSet.MetersPerUnit = GridSetFactory.Epsg3857ToMeters;
                    // TODO:
                    gridSet.Extent = GridSetFactory.ProjectedWorld;
                    break;
                default:
                    throw new ArgumentException("无法识别的坐标系");
            }

            var index = 0;
            foreach (var tileMatrix in capabilities.Contents.TileMatrixSet.TileMatrices)
            {
                var scaleDenominator = tileMatrix.ScaleDenominator;
                var resolution = gridSet.PixelSize * (scaleDenominator / gridSet.MetersPerUnit);
                var grid = new Grid(index)
                {
                    Name = tileMatrix.Identifier,
                    NumTilesWidth = tileMatrix.MatrixWidth,
                    NumTilesHeight = tileMatrix.MatrixHeight,
                    ScaleDenominator = scaleDenominator,
                    Resolution = resolution
                };
                gridSet.AppendGrid(grid);
                index++;
            }

            (GridSet GridSet, CoordinateSystem CoordinateSystem, Envelope Extent ) tuple = (gridSet, crs, extent);
            entry.SetValue(tuple);
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            return tuple;
        });

        GridSet = tuple.GridSet;
        if (GridSet == null)
        {
            throw new ArgumentException("无法获取 WMTS 的配置信息");
        }

        MatrixSet = GridSet.Name;
        CoordinateSystem = tuple.CoordinateSystem;
        Srid = GridSet.SRID;
        Envelope = tuple.Extent;
    }

    public async Task<ImageData> GetImageAsync(string matrix, int row, int col)
    {
        var path = $"{_cacheFolder}/{matrix}_{row}_{col}.png";
        if (File.Exists(path))
        {
            var fileBytes = await File.ReadAllBytesAsync(path);
            return new ImageData(fileBytes, ImageDataType.Image);
        }

        var requestUrl = string.Format(url, matrix, row, col);
        var response = await SendAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(path, bytes);
        return new ImageData(bytes, ImageDataType.Image);
    }

    private async Task<HttpResponseMessage> SendAsync(string requestUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.TryAddWithoutValidation("Accept", "*/*");
        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
        request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
        request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
        request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        request.Headers.TryAddWithoutValidation("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
        request.Headers.TryAddWithoutValidation("Cookie",
            "HWWAFSESID=1eec21ebe50bd5c69672; HWWAFSESTIME=1728960350659");
        request.Headers.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.0");
        request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
        var httpClient = HttpClientFactory == null ? HttpClient : HttpClientFactory.CreateClient(Name);
        var response = await httpClient.SendAsync(request);
        return response;
    }

    public void Dispose()
    {
    }

    public ISource Clone()
    {
        return (ISource)MemberwiseClone();
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex SridRegex();
}