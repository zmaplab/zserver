// using System.Diagnostics;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Text;
// using BitMiracle.LibTiff.Classic;
// using MessagePack;
// using Microsoft.Extensions.Caching.Memory;
// using Microsoft.Extensions.Logging;
// using NetTopologySuite.Geometries;
// using ProjNet.CoordinateSystems;
// using ZMap.Infrastructure;
// using ZMap.Source.CloudOptimizedGeoTIFF;
// using ZMap.TileGrid;
//
// namespace ZMap.Source;
//
// public class RemoteCOGGeoTiffSource(string url) : ITiledSource, IRemoteHttpSource
// {
//     private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<RemoteCOGGeoTiffSource>);
//     private string _cacheFolder;
//     public string Key { get; set; }
//     public string Name { get; set; }
//     public int Srid { get; private set; }
//     public CoordinateSystem CoordinateSystem { get; private set; }
//     public Envelope Envelope { get; private set; }
//     public GridSet GridSet { get; set; }
//     public IHttpClientFactory HttpClientFactory { get; set; }
//     public int MaxZoom { get; set; }
//
//     /// <summary>
//     /// 不同云平台可以返回不同的授权 URL
//     /// </summary>
//     /// <returns></returns>
//     protected virtual string GetUrl()
//     {
//         return url;
//     }
//
//     public async Task LoadAsync()
//     {
//         _cacheFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache/wmts/remote/{Name}");
//         var tuple = await Cache.GetOrCreateAsync($"{url}_Capabilities", async entry =>
//         {
//             var uri = new Uri(GetUrl());
//             var request = new HttpRequestMessage(HttpMethod.Get, uri);
//             request.Headers.Range = new RangeHeaderValue(0, 1024 * 64);
//             var httpClient = HttpClientFactory.CreateClient(uri.Host);
//             var response = await httpClient.SendAsync(request);
//             response.EnsureSuccessStatusCode();
//             var bytes = await response.Content.ReadAsByteArrayAsync();
//             using var stream = new MemoryStream(bytes);
//
//             var tuple = GeoTIFFUtility.ReadTiffInfo(stream);
//             entry.SetValue(tuple);
//             entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
//             return tuple;
//         });
//         GridSet = tuple.GridSet;
//         if (GridSet == null)
//         {
//             throw new ArgumentException("无法获取栅格配置信息");
//         }
//
//         CoordinateSystem = tuple.CoordinateSystem;
//         Srid = GridSet.SRID;
//         Envelope = tuple.GridSet.Extent;
//         MaxZoom = tuple.MaxZoom;
//     }
//
//     public ISource Clone()
//     {
//         return (ISource)MemberwiseClone();
//     }
//
//     public async Task<ImageData> GetImageAsync(string matrix, int col, int row)
//     {
//         var stopwatch = Stopwatch.StartNew();
//         stopwatch.Start();
//
//         var z = int.Parse(matrix);
//         if (z > MaxZoom)
//         {
//             return null;
//         }
//
//         if (!GridSet.GridLevels.TryGetValue(matrix, out var grid))
//         {
//             return null;
//         }
//
//         if (col >= grid.NumTilesWidth || row >= grid.NumTilesHeight)
//         {
//             return null;
//         }
//
//         var tuple = Utility.GetTiffPath(Name, matrix, row, col);
//
// #if !DEBUG
//         if (File.Exists(tuple.FullPath))
//         {
//             stopwatch.Stop();
//             Logger.Value.LogInformation("GetImage {TileRow} {TileCol} {TileMatrix}: {ElapsedMilliseconds}ms, CACHED",
//                 row,
//                 col,
//                 matrix, stopwatch.ElapsedMilliseconds);
//             var cachedBytes = await File.ReadAllBytesAsync(tuple.FullPath);
//             var array = MessagePackSerializer.Deserialize<int[]>(cachedBytes);
//             return new ImageData(array, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
//         }
// #endif
//
//         var tileNumber = (short)grid.Index;
//
//
//         var pixelX = col * GridSet.TileWidth;
//         var pixelY = row * GridSet.TileHeight;
//
//         var tileBuffer = new int[GridSet.TileWidth * GridSet.TileHeight];
//
//         if (!tiff.ReadRGBATile(pixelX, pixelY, tileBuffer))
//         {
//             return null;
//         }
//         
//         var uri = new Uri(GetUrl());
//         var request = new HttpRequestMessage(HttpMethod.Get, uri);
//         request.Headers.Range = new RangeHeaderValue(0, 1024 * 64);
//         var httpClient = HttpClientFactory.CreateClient(uri.Host);
//         var response = await httpClient.SendAsync(request);
//         response.EnsureSuccessStatusCode();
//         var bytes = await response.Content.ReadAsByteArrayAsync();
//
//         stopwatch.Stop();
//         Logger.Value.LogInformation("ReadRGBATile {TileRow} {TileCol} {TileMatrix}: {ElapsedMilliseconds}ms", row, col,
//             matrix, stopwatch.ElapsedMilliseconds);
//
//         if (GridSet.YCoordinateFirst)
//         {
//             GeoTIFFUtility.FlipImageVertically(tileBuffer, GridSet.TileWidth, GridSet.TileHeight);
//         }
//
//         var directory = Path.GetDirectoryName(tuple.FullPath);
//         if (!string.IsNullOrEmpty(directory))
//         {
//             if (!Directory.Exists(directory))
//             {
//                 Directory.CreateDirectory(directory);
//             }
//
//             if (!string.IsNullOrEmpty(tuple.FullPath))
//             {
//                 var data = MessagePackSerializer.Serialize(tileBuffer);
//                 await File.WriteAllBytesAsync(tuple.FullPath, data);
//             }
//         }
//
//         return new ImageData(tileBuffer, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
//     }
//
//     public void Dispose()
//     {
//     }
// }