using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using MessagePack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using TagImageFileFormat;
using ZMap.Infrastructure;
using ZMap.TileGrid;
using ArgumentException = System.ArgumentException;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

/// <summary>
/// gdal_translate world.tif world_webmerc_cog.tif -co TILED=YES -of COG -co TILING_SCHEME=GoogleMapsCompatible -co COMPRESS=JPEG -co BIGTIFF=YES
/// gdal_translate 前童镇_DOM.tif 前童镇_DOM_webmerc_cog2.tif -co TILED=YES -co COMPRESS=JPEG -co BIGTIFF=YES  -co  TILING_SCHEME=GoogleMapsCompatible
/// rio cogeo create 北仑区.tif 北仑区_webmerc_cog.tif --blocksize 256
/// 
/// </summary>
public class COGGeoTiffSource : ITiledSource
{
    private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<COGGeoTiffSource>);
    private static readonly Dictionary<string, MemoryMappedFile> MemoryMappedFiles = new();

    // private static readonly object StreamLocker = new();
    // private static readonly Dictionary<string, Tiff> OpenedTiffDict = new();

    private readonly string _file;
    public string Key { get; set; }
    public string Name { get; set; }
    public int Srid { get; set; }
    public CoordinateSystem CoordinateSystem { get; set; }
    public Envelope Envelope { get; set; }
    public GridSet GridSet { get; set; }
    public int MaxZoom { get; set; }

    static COGGeoTiffSource()
    {
        // Tiff.SetErrorHandler(new EmptyTiffErrorHandler());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    public COGGeoTiffSource(string file)
    {
        _file = file;
    }

    public Task LoadAsync()
    {
        if (GridSet != null)
        {
            return Task.CompletedTask;
        }

        var tuple = Cache.GetOrCreate($"{_file}_COGGeoTiffSource", entry =>
        {
            if (!File.Exists(_file))
            {
                throw new FileNotFoundException($"Source file '{_file}' was not found.");
            }

            using var stream = File.OpenRead(_file);
            // using var stream2 = File.OpenRead(_file);

            // var tuple2 = LibTiffUtility.ReadTiffInfo(stream2);
            var tuple = TIFFUtility.ReadTiffInfo(stream);
            // if (!tiff.IsTiled())
            // {
            //     throw new ArgumentException("文件不是 COG 文件");
            // }
            // var rowsPerStripFieldValue = tiff.GetField(TiffTag.ROWSPERSTRIP)?.ElementAtOrDefault(0);
            // var rowsPerStrip = rowsPerStripFieldValue?.ToInt() ?? 0;
            // if (rowsPerStrip > 0)
            // {
            //     throw new ArgumentException("文件不是 COG 文件");
            // }

            var path = Utility.GetTiffPath(_file, "0", 0, 0);
            var directory = Path.GetDirectoryName(path.FullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            entry.SetValue(tuple);
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            return tuple;
        });

        GridSet = tuple.GridSet;
        if (GridSet == null)
        {
            throw new ArgumentException("无法获取栅格配置信息");
        }

        CoordinateSystem = tuple.CoordinateSystem;
        Srid = GridSet.SRID;
        Envelope = tuple.GridSet.Extent;
        MaxZoom = tuple.MaxZoom;

        return Task.CompletedTask;
    }

    public async Task<ImageData> GetImageAsync(string matrix, int col, int row)
    {
        var stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        var z = int.Parse(matrix);
        if (z > MaxZoom)
        {
            return null;
        }

        if (!GridSet.GridLevels.TryGetValue(matrix, out var grid))
        {
            return null;
        }

        if (col >= grid.NumTilesWidth || row >= grid.NumTilesHeight)
        {
            return null;
        }

        var tuple = Utility.GetTiffPath(_file, matrix, row, col);

#if !DEBUG
        if (File.Exists(tuple.FullPath))
        {
            stopwatch.Stop();
            Logger.Value.LogInformation("GetImage {TileRow} {TileCol} {TileMatrix}: {ElapsedMilliseconds}ms, CACHED",
                row,
                col,
                matrix, stopwatch.ElapsedMilliseconds);
            var cachedBytes = await File.ReadAllBytesAsync(tuple.FullPath);
            var array = MessagePackSerializer.Deserialize<int[]>(cachedBytes);
            return new ImageData(array, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
        }
#endif

        var tileNumber = (short)grid.Index;

        using var accessor = OpenViewAccessor();
        var byteAccessor = new MemoryMappedViewByteAccessor(accessor);

        var metadata =
            Cache.Get<(TIFF Tiff, GridSet GridSet, CoordinateSystem CoordinateSystem, int MaxZoom )>(
                $"{_file}_COGGeoTiffSource");
        var tiff = metadata.Tiff;
        // using var tiff = Tiff.Open(_file, "r");
        //
        // try
        // {
        //     tiff.SetDirectory(tileNumber);
        // }
        // catch (Exception e)
        // {
        //     Logger.Value.LogError(e, "SetDirectory {TileRow} {TileCol} {TileMatrix}", row, col, matrix);
        //     return null;
        // }
        //
        // var pixelX = col * GridSet.TileWidth;
        // var pixelY = row * GridSet.TileHeight;
        //
        // var tileBuffer = new int[GridSet.TileWidth * GridSet.TileHeight];
        //
        // if (!tiff.ReadRGBATile(pixelX, pixelY, tileBuffer))
        // {
        //     return null;
        // }
        //
        // if (GridSet.YCoordinateFirst)
        // {
        //     LibTiffUtility.FlipImageVertically(tileBuffer, GridSet.TileWidth, GridSet.TileHeight);
        // }

        var tileBuffer = await tiff.GetTileAsync(byteAccessor, tileNumber, col, row);

        stopwatch.Stop();
        Logger.Value.LogInformation("ReadRGBATile {TileRow} {TileCol} {TileMatrix}: {ElapsedMilliseconds}ms", row, col,
            matrix, stopwatch.ElapsedMilliseconds);

        if (string.IsNullOrEmpty(tuple.FullPath) || File.Exists(tuple.FullPath))
        {
            return new ImageData(tileBuffer, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
        }

        var data = MessagePackSerializer.Serialize(tileBuffer);
        await File.WriteAllBytesAsync(tuple.FullPath, data);

        return new ImageData(tileBuffer, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
    }

    private Stream OpenStream()
    {
        lock (MemoryMappedFiles)
        {
            if (MemoryMappedFiles.TryGetValue(_file, out var memoryMappedFile))
            {
                return memoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            }

            memoryMappedFile =
                MemoryMappedFile.CreateFromFile(_file, FileMode.Open, null, 0L, MemoryMappedFileAccess.Read);
            MemoryMappedFiles.Add(_file, memoryMappedFile);

            return memoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
        }
    }

    private MemoryMappedViewAccessor OpenViewAccessor()
    {
        lock (MemoryMappedFiles)
        {
            if (MemoryMappedFiles.TryGetValue(_file, out var memoryMappedFile))
            {
                return memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            }

            memoryMappedFile =
                MemoryMappedFile.CreateFromFile(_file, FileMode.Open, null, 0L, MemoryMappedFileAccess.Read);
            MemoryMappedFiles.Add(_file, memoryMappedFile);

            return memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        }
    }

    public ISource Clone()
    {
        return (ISource)MemberwiseClone();
    }

    public void Dispose()
    {
    }
}