using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using BitMiracle.LibTiff.Classic;
using MessagePack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ZMap.Infrastructure;
using ZMap.TileGrid;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

/// <summary>
/// gdal_translate world.tif world_webmerc_cog.tif -co TILED=YES -of COG -co TILING_SCHEME=GoogleMapsCompatible -co COMPRESS=JPEG -co BIGTIFF=YES
/// gdal_translate 前童镇_DOM.tif 前童镇_DOM_webmerc_cog2.tif -co TILED=YES -co COMPRESS=JPEG -co BIGTIFF=YES
/// </summary>
public class COGGeoTiffSource : ITiledSource
{
    private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<COGGeoTiffSource>);
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
        Tiff.SetErrorHandler(new EmptyTiffErrorHandler());
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

        var tuple = Cache.GetOrCreate($"{_file}_gridSet", entry =>
        {
            if (!File.Exists(_file))
            {
                throw new FileNotFoundException($"Source file '{_file}' was not found.");
            }

            using var tiff = Tiff.Open(_file, "r");
            if (!tiff.IsTiled())
            {
                throw new ArgumentException("文件不是 COG 文件");
            }
            // var rowsPerStripFieldValue = tiff.GetField(TiffTag.ROWSPERSTRIP)?.ElementAtOrDefault(0);
            // var rowsPerStrip = rowsPerStripFieldValue?.ToInt() ?? 0;
            // if (rowsPerStrip > 0)
            // {
            //     throw new ArgumentException("文件不是 COG 文件");
            // }

            var coordinateSystem = GetCoordinateSystem(_file, tiff);

            var unit = coordinateSystem.GetUnits(0);

            var measurePerUnit = unit is LinearUnit linearUnit
                ? linearUnit.MetersPerUnit
                : (unit as AngularUnit)!.RadiansPerUnit * 180 / Math.PI;

            var directories = tiff.NumberOfDirectories();
            List<short> levels = [];
            for (short i = 0; i < directories; i++)
            {
                tiff.SetDirectory(i);
                if (!IsMaskImage(tiff))
                {
                    levels.Add(i);
                }
            }

            var gridSet = new GridSet
            {
                Name = coordinateSystem.Name,
                YBaseToggle = false,
                YCoordinateFirst = true,
                MetersPerUnit = measurePerUnit,
                // // TODO: 如果是度的系统，能否正常工作？
                // PixelSize = extent.Width / topWidthHeight.Width,
                PixelSize = GridSetFactory.DefaultPixelSizeMeter,
                SRID = (int)coordinateSystem.AuthorityCode
            };

            var maxLevel = 0;
            if (levels.Count == 0)
            {
                gridSet.Extent = new Envelope(double.MinValue, double.MinValue, double.MinValue, double.MinValue);
            }
            else
            {
                var minLevel = levels[0];
                tiff.SetDirectory(minLevel);
                var topOrigin = GetOrigin(tiff);
                var topResolution = GetCurrentResolution(tiff, null, 0, 0);
                var topWidthHeight = GetWidthHeight(tiff);
                var topExtent = GetExtent(tiff, topOrigin, topResolution, topWidthHeight.Width,
                    topWidthHeight.Height);
                if (topExtent == null)
                {
                    throw new ArgumentException("读取 tiff 的范围失败");
                }

                var extent = new Envelope(topExtent[0], topExtent[2], topExtent[1], topExtent[3]);
                gridSet.Extent = extent;

                foreach (var zoomLevel in levels)
                {
                    tiff.SetDirectory(zoomLevel);

                    var resolution =
                        GetCurrentResolution(tiff, topResolution, topWidthHeight.Width, topWidthHeight.Height);

                    var width = resolution.Length == 4
                        ? (int)resolution[2]
                        : tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    var height = resolution.Length == 4
                        ? (int)resolution[3]
                        : tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    var tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                    var tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();

                    if (gridSet.TileWidth == 0)
                    {
                        gridSet.TileWidth = tileWidth;
                    }

                    if (gridSet.TileHeight == 0)
                    {
                        gridSet.TileHeight = tileHeight;
                    }

                    var grid = new Grid(zoomLevel)
                    {
                        Name = zoomLevel.ToString(),
                        Resolution = resolution[0],
                        ScaleDenominator = resolution[0] / gridSet.PixelSize * gridSet.MetersPerUnit,
                        NumTilesWidth = width / tileWidth + (width % tileWidth == 0 ? 0 : 1),
                        NumTilesHeight = height / tileHeight + (height % tileHeight == 0 ? 0 : 1),
                        Width = width,
                        Height = height,
                        TileHeight = tileHeight,
                        TileWidth = tileWidth
                    };
                    var number = grid.NumTilesWidth * grid.NumTilesHeight;
                    var numberOfTiles = tiff.NumberOfTiles();

                    if (number != numberOfTiles)
                    {
                        throw new ArgumentException("瓦片数量不匹配");
                    }

                    gridSet.AppendGrid(grid);
                }

                maxLevel = int.Parse(gridSet.GridLevels.Keys.Max());

                var dataType = (SampleFormat)tiff.GetField(TiffTag.SAMPLEFORMAT)[0].Value;
                var compression = (Compression)tiff.GetField(TiffTag.COMPRESSION)[0].Value;
                var sb = new StringBuilder();
                for (var i = 0; i <= maxLevel; ++i)
                {
                    var key = i.ToString();
                    var grid = gridSet.GridLevels[key];
                    if (i != 0)
                    {
                        sb.Append("    ");
                    }

                    sb.Append(key).Append("    ").Append(grid.Width).Append("x").Append(grid.Height)
                        .Append("    ")
                        .Append(grid.TileWidth).Append("x")
                        .Append(grid.TileHeight)
                        .Append("    ")
                        .Append(grid.NumTilesWidth).Append("x").Append(grid.NumTilesHeight).Append("(")
                        .Append(grid.NumTilesWidth * grid.NumTilesHeight).Append(")").Append("    ")
                        .Append(Math.Round(grid.Resolution, 4))
                        .AppendLine();
                }

                var fileInfo = new FileInfo(_file);

                var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                var info = $"""

                            COG File Info - file://{fileInfo.FullName}
                                Tiff type       Tiff (v42)
                                DataType        {dataType}
                                Size            {size} MB

                            Images
                                Compression     {compression}
                                Origin          {Math.Round(topOrigin[0], 4)}, {Math.Round(topOrigin[1], 4)}, {Math.Round(topOrigin[2], 4)}
                                Resolution      {Math.Round(topResolution[0], 4)}, {Math.Round(topResolution[1], 4)}, {Math.Round(topResolution[2], 4)}
                                BoundingBox     {Math.Round(topExtent[0], 4)}, {Math.Round(topExtent[1], 4)}, {Math.Round(topExtent[2], 4)}, {Math.Round(topExtent[3], 4)}
                                EPSG            EPSG:{gridSet.SRID} {coordinateSystem.Name} https://epsg.io/{gridSet.SRID}
                                Images
                                Id  Size        Tile Size    Tile Count     Resolution
                                {sb}
                            """;
                Logger.Value.LogInformation(info);
            }

            (GridSet GridSet, CoordinateSystem CoordinateSystem, int MaxZoom ) tuple = (gridSet, coordinateSystem,
                maxLevel);
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


    public async Task<ImageData> GetImageAsync(string matrix, int row, int col)
    {
        var stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        var z = int.Parse(matrix);
        if (z > MaxZoom)
        {
            return null;
        }

        var x = row;
        var y = col;

        if (!GridSet.GridLevels.TryGetValue(matrix, out var grid))
        {
            return null;
        }

        var tuple = Utility.GetTiffPath(_file, matrix, row, col);

#if !DEBUG
        if (File.Exists(tuple.FullPath))
        {
            stopwatch.Stop();
            Logger.Value.LogInformation("GetImage {TileRow} {TileCol} {TileMatrix}: {ElapsedMilliseconds}ms, CACHED", row,
                col,
                matrix, stopwatch.ElapsedMilliseconds);
            var cachedBytes = await File.ReadAllBytesAsync(tuple.FullPath);
            var array = MessagePackSerializer.Deserialize<int[]>(cachedBytes);
            return new ImageData(array, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
        }
#endif

        var tileNumber = (short)grid.Index;

        // Tiff tiff;
        // lock (StreamLocker)
        // {
        //     if (OpenedTiffDict.TryGetValue(_file, out var value))
        //     {
        //         tiff = value;
        //     }
        //     else
        //     {
        //         var v = Tiff.Open(_file, "r");
        //         // var mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.Open);
        //         // var stream = mmf.CreateViewStream();
        //         // var v = Tiff.ClientOpen(_file, "r", stream, new TiffStream());
        //         OpenedTiffDict.Add(_file, v);
        //         tiff = v;
        //     }
        // }

        using var tiff = Tiff.Open(_file, "r");

        try
        {
            tiff.SetDirectory(tileNumber);
        }
        catch (Exception e)
        {
            Logger.Value.LogError(e, "SetDirectory {TileRow} {TileCol} {TileMatrix}", row, col, matrix);
            return null;
        }

        var pixelX = x * GridSet.TileWidth;
        var pixelY = y * GridSet.TileHeight;

        var tileBuffer = new int[GridSet.TileWidth * GridSet.TileHeight];

        if (!tiff.ReadRGBATile(pixelX, pixelY, tileBuffer))
        {
            return null;
        }

        stopwatch.Stop();
        Logger.Value.LogInformation("ReadRGBATile {TileRow} {TileCol} {TileMatrix}: {ElapsedMilliseconds}ms", row, col,
            matrix, stopwatch.ElapsedMilliseconds);

        if (GridSet.YCoordinateFirst)
        {
            FlipImageVertically(tileBuffer, GridSet.TileWidth, GridSet.TileHeight);
        }

        var directory = Path.GetDirectoryName(tuple.FullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!string.IsNullOrEmpty(tuple.FullPath))
            {
                var data = MessagePackSerializer.Serialize(tileBuffer);
                await File.WriteAllBytesAsync(tuple.FullPath, data);
            }
        }

        return new ImageData(tileBuffer, ImageDataType.Pixels, GridSet.TileWidth, GridSet.TileHeight);
    }

    public ISource Clone()
    {
        return (ISource)MemberwiseClone();
    }

    public void Dispose()
    {
    }

    private CoordinateSystem GetCoordinateSystem(string path, Tiff tiff)
    {
        // 按文档，第一优先级是从 sidecar file 中获取
        var srsWkt = GetSRSFromSidecarFile(path);
        var coordinateSystem = srsWkt == null ? null : new CoordinateSystemFactory().CreateFromWkt(srsWkt);

        if (coordinateSystem != null)
        {
            return coordinateSystem;
        }

        var geoKeys = GeoInfo.Read(tiff);
        var srid = Convert.ToInt32(geoKeys[3072].Value);
        coordinateSystem = CoordinateReferenceSystem.Get(srid);
        if (coordinateSystem != null)
        {
            return coordinateSystem;
        }

        var crsName = geoKeys[1026].Value.ToString();
        return CoordinateReferenceSystem.Get(crsName);
    }

    private string GetSRSFromSidecarFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var sidecarFilePath = $"{path}.aux.xml";
        if (!File.Exists(sidecarFilePath))
        {
            return null;
        }

        var xmlText = File.ReadAllText(sidecarFilePath);

        var doc = XDocument.Parse(xmlText);

        if (doc.Root == null)
        {
            return null;
        }

        var srs = doc.Root.Element("SRS")?.Value;
        return srs;
    }

    private double[] GetExtent(Tiff tiff, double[] topOrigin, double[] topResolution,
        int topWidth, int topHeight)
    {
        var transform = tiff.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG);

        if (transform != null)
        {
            if (transform[0].ToInt() != 16) throw new Exception(); //todo
            var matrix = transform[1].ToDoubleArray();

            var a = matrix[0];
            var b = matrix[1];
            // var c = matrix[2];
            var d = matrix[3];
            var e = matrix[4];
            var f = matrix[5];
            // var g = matrix[6];
            var h = matrix[7];

            var corners = new[]
            {
                [0, 0],
                [0, topHeight],
                [topWidth, 0],
                new double[] { topWidth, topHeight }
            };
            var projected = corners.Select(x =>
            {
                var i = x[0];
                var j = x[1];

                return new[] { d + a * i + b * j, h + e * i + f * j };
            }).ToList();
            var xs = projected.Select(p => p[0]).ToList();
            var ys = projected.Select(p => p[1]).ToList();

            return
            [
                xs.Min(),
                ys.Min(),
                xs.Max(),
                ys.Max()
            ];
        }

        if (topResolution == null)
        {
            return null;
        }

        var x1 = topOrigin[0];
        var y1 = topOrigin[1];

        var x2 = x1 + topResolution[0] * topWidth;
        var y2 = y1 + topResolution[1] * topHeight;

        return
        [
            Math.Min(x1, x2),
            Math.Min(y1, y2),
            Math.Max(x1, x2),
            Math.Max(y1, y2)
        ];
    }

    private (int Width, int Height) GetWidthHeight(Tiff image)
    {
        var width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
        var height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
        return (width, height);
    }

    private double[] GetOrigin(Tiff tif)
    {
        var modelTilePoint = tif.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
        var modelTransformation = tif.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG);

        if (modelTilePoint is not null && modelTilePoint[0].ToInt() == 6)
        {
            var array = modelTilePoint[1].ToDoubleArray();
            return
            [
                array[3],
                array[4],
                array[5]
            ];
        }

        if (modelTransformation != null)
        {
            var transformArr = modelTransformation[1].ToDoubleArray();
            return
            [
                transformArr[3],
                transformArr[7],
                transformArr[11]
            ];
        }

        throw new Exception("The image does not have an affine transformation.");
    }

    // private double[] GetResolution(Tiff image, short imageNumber)
    // {
    //     image.SetDirectory(imageNumber);
    //     var resolution = GetResolutionMetadata(image);
    //     if (resolution == null)
    //     {
    //         throw new Exception(
    //             $"Couldn't get transformation tag or modelpixel tag from sub directory {imageNumber} of {_path}");
    //     }
    //
    //     return resolution;
    // }

    private double[] GetResolution(Tiff image)
    {
        var modelPixelScale = image.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
        var transformation = image.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG);

        if (modelPixelScale != null)
        {
            var scaleArr = modelPixelScale[1].ToDoubleArray();
            var res = new[]
            {
                scaleArr[0],
                -scaleArr[1],
                scaleArr[2],
            };
            return res;
        }

        if (transformation == null)
        {
            return null;
        }

        var matrix = transformation[1].ToDoubleArray();
        if (matrix[1] == 0 && matrix[4] == 0)
        {
            return
            [
                matrix[0],
                -matrix[5],
                matrix[10]
            ];
        }

        return
        [
            Math.Sqrt(matrix[0] * matrix[0] + matrix[4] * matrix[4]),
            -Math.Sqrt(matrix[1] * matrix[1] + matrix[5] * matrix[5]),
            matrix[10]
        ];
    }

    private double[] GetCurrentResolution(Tiff image, double[] topResolution, int topWidth, int topHeight)
    {
        var resolution = GetResolution(image);
        if (resolution != null)
        {
            return resolution;
        }

        if (topResolution == null)
        {
            return null;
        }

        var width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
        var height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

        var globalResX = topResolution[0];
        var globalResY = topResolution[1];

        var resX = globalResX * topWidth / width;
        var resY = globalResY * topHeight / height;

        return [resX, resY, width, height];
    }

    private bool IsMaskImage(Tiff image)
    {
        // Get the NewSubfileType tag
        var value = image.GetField(TiffTag.SUBFILETYPE);
        var type = 0;
        if (value != null)
        {
            // Convert the tag value to an integer
            type = value[0].ToInt();
        }

        // Check if the third bit (bit 2, 0-indexed) of NewSubfileType is set
        return (type & 4) == 4;
    }

    private void FlipImageVertically(int[] pixels, int width, int height)
    {
        // 检查输入的有效性
        if (pixels == null || pixels.Length != width * height)
        {
            throw new ArgumentException("Invalid pixel array or dimensions.");
        }

        // 遍历图像的一半高度
        for (var row = 0; row < height / 2; row++)
        {
            // 计算当前行的开始索引和与之对称的底部行的开始索引
            var topRowStartIndex = row * width;
            var bottomRowStartIndex = (height - row - 1) * width;

            // 交换当前行和底部行的所有像素
            for (var col = 0; col < width; col++)
            {
                // 计算当前列在顶部行和底部行中的索引
                var topIndex = topRowStartIndex + col;
                var bottomIndex = bottomRowStartIndex + col;

                var bottomPixel = pixels[bottomIndex];
                var topPixel = pixels[topIndex];

                pixels[topIndex] = HandleDefault(bottomPixel);
                pixels[bottomIndex] = HandleDefault(topPixel);
            }
        }
    }

    private int HandleDefault(int color)
    {
        // var alpha = (color >> 24) & 0xff;
        // var red = (color >> 16) & 0xff;
        // var green = (color >> 8) & 0xff;
        // var blue = color & 0xff;
        // if (red == 255 && green == 255 && blue == 255)
        // {
        //     return 0;
        // }
        //
        // if (red == 0 && green == 0 && blue == 0)
        // {
        //     return 0;
        // }

        // comments: -1        -> 255.255.255
        //           -16777216 -> 0.0.0
        return color is -1 or -16777216 ? 0 : color;
    }
}