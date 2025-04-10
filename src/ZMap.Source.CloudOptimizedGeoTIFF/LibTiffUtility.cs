using System.Text;
using BitMiracle.LibTiff.Classic;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ZMap.Infrastructure;
using ZMap.TileGrid;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

public static class LibTiffUtility
{
    private static readonly Lazy<ILogger> Logger =
        new(Log.CreateLogger("ZMap.Source.CloudOptimizedGeoTIFF.LibTiffUtility"));

    /// <summary>
    /// 1024 1 表示投影坐标系，2 表示地理经纬系，3 表示地心(X,Y,Z)坐标系
    /// 2048 地理类型代码，指出采用哪一个地理坐标系统。值为 32767 表示为自定义的坐标系。
    /// </summary>
    /// <param name="tiff"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    internal static Dictionary<int, GeoKeyEntry> ReadGeoKeyEntries(Tiff tiff)
    {
        var geoKeyDirectory = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
        var geoDoubleParamsFieldValue =
            tiff.GetField(TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG)?.ElementAtOrDefault(1).Value as byte[] ?? [];
        var geoAsciiParamsFieldValue =
            tiff.GetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG)?.ElementAtOrDefault(1).Value as byte[] ?? [];

        var doubleParams = new double[geoDoubleParamsFieldValue.Length / 8];
        for (var i = 0; i < doubleParams.Length; ++i)
        {
            doubleParams[i] = BitConverter.ToDouble(geoDoubleParamsFieldValue, i * 8);
        }

        var bytes = geoKeyDirectory[1].ToByteArray();
        // var v1 = BitConverter.ToUInt16(bytes, 0);
        // var v2 = BitConverter.ToUInt16(bytes, 2);
        // var v3 = BitConverter.ToUInt16(bytes, 4);
        var numGeoKeys = BitConverter.ToUInt16(bytes, 6);

        var geoKeys = new Dictionary<int, GeoKeyEntry>();
        var start = 8;
        for (var i = 0; i < numGeoKeys; i++)
        {
            if (bytes.Length <= start + 8)
            {
                break;
            }

            var keyId = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var location = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var count = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var offset = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var entry = new GeoKeyEntry
            {
                KeyId = keyId,
                Location = location,
                Count = count,
                ValueOrOffset = offset
            };
            if (location != 0)
            {
                var end = count - 1 + offset;
                switch (location)
                {
                    case 34737:
                    {
                        var piece = Encoding.ASCII.GetString(geoAsciiParamsFieldValue[offset..end]).Split('=');
                        entry.Value = piece.Length == 1 ? piece[0] : piece[1].Trim();
                        break;
                    }
                    case 34736:
                    {
                        entry.Value = doubleParams[offset];
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException("不支持的数据地址");
                    }
                }
            }
            else
            {
                entry.Value = offset;
            }

            // (keyId, location, count, value)
            geoKeys.Add(keyId, entry);
        }

        return geoKeys;
    }

    internal static void FlipImageVertically(int[] pixels, int width, int height)
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

    public static (GridSet GridSet, CoordinateSystem CoordinateSystem, int MaxZoom ) ReadTiffInfo(Stream stream)
    {
        string name;
        if (stream is FileStream fileStream)
        {
            name = fileStream.Name;
        }
        else
        {
            name = Guid.NewGuid().ToString("N");
        }

        using Tiff tiff = Tiff.ClientOpen(name, "r", stream, new TiffStream());

        var coordinateSystem = GetCoordinateSystem(name, tiff);

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
                    Height = height
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


            var size = Math.Round((double)stream.Length / 1024 / 1024, 2);
            var info = $"""

                        COG File Info - file://{name}
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
        return tuple;
    }

    private static CoordinateSystem GetCoordinateSystem(string path, Tiff tiff)
    {
        // 按文档，第一优先级是从 sidecar file 中获取
        var srsWkt = TIFFUtility.GetSRSFromSidecarFile(path);
        var coordinateSystem = srsWkt == null ? null : TIFFUtility.CoordinateSystemFactory.CreateFromWkt(srsWkt);

        if (coordinateSystem != null)
        {
            return coordinateSystem;
        }

        var geoKeys = ReadGeoKeyEntries(tiff);
        var decoder = new GeoTiffIIOMetadataDecoder(geoKeys);

        var modelType = decoder.GetModelType(); // 1 表示投影坐标系, 2 表示地理经纬系， 3 表示地心(X,Y,Z)坐标系
        if (GeoTiffPCSCodes.ModelTypeProjected == modelType)
        {
            coordinateSystem = TIFFUtility.CreateProjectedCoordinateReferenceSystem(decoder);
        }
        else if (GeoTiffGCSCodes.ModelTypeGeographic == modelType)
        {
            coordinateSystem = TIFFUtility.CreateGeographicCoordinateReferenceSystem(decoder);
        }
        else
        {
            throw new NotSupportedException("不支持的坐标系类型");
        }

        if (coordinateSystem == null)
        {
            throw new ArgumentException("无法获取坐标系信息");
        }

        // if (geoKeys.TryGetValue(1024, out var citation))
        // {
        //     var crsName = citation.Value.ToString();
        //     return CoordinateReferenceSystem.Get(crsName);
        // }

        return coordinateSystem;
    }

    private static double[] GetExtent(Tiff tiff, double[] topOrigin, double[] topResolution,
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

    private static (int Width, int Height) GetWidthHeight(Tiff image)
    {
        var width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
        var height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
        return (width, height);
    }

    private static double[] GetOrigin(Tiff tif)
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

    private static double[] GetResolution(Tiff image)
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

    private static double[] GetCurrentResolution(Tiff image, double[] topResolution, int topWidth, int topHeight)
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

    private static int HandleDefault(int color)
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

    private static bool IsMaskImage(Tiff image)
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
}

public class EmptyTiffErrorHandler : TiffErrorHandler
{
    public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
    {
    }

    public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format,
        params object[] args)
    {
    }
}