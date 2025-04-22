using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Projections;
using ProjNet.CoordinateSystems.Transformations;
using TagImageFileFormat;
using ZMap.Infrastructure;
using ZMap.TileGrid;
using static ZMap.Source.CloudOptimizedGeoTIFF.GeoTiffConstants;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

internal static class TIFFUtility
{
    private static readonly Lazy<ILogger> Logger =
        new(Log.CreateLogger("ZMap.Source.CloudOptimizedGeoTIFF.TIFFUtility"));

    internal static readonly CoordinateSystemFactory CoordinateSystemFactory = new();
    internal static readonly CoordinateTransformationFactory CoordinateTransformationFactory = new();

    public static (TIFF Tiff, GridSet GridSet, CoordinateSystem CoordinateSystem, int MaxZoom )
        ReadTiffInfo(Stream stream)
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

        using var reader = new BinaryReader(stream);
        var tiff = TIFF.Load(stream);

        var coordinateSystem = GetCoordinateSystem(name, tiff, reader);

        var unit = coordinateSystem.GetUnits(0);

        var measurePerUnit = unit is LinearUnit linearUnit
            ? linearUnit.MetersPerUnit
            : (unit as AngularUnit)!.RadiansPerUnit * 6378137.0;

        var directories = tiff.ImageFileDirectories.Count;
        List<int> levels = [];
        for (var i = 0; i < directories; i++)
        {
//            var ifd = tiff.ImageFileDirectories.ElementAt(i);
            // if (!IsMaskImage(ifd))
            // {
            //     continue;
            // }

            levels.Add(i);
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
            var topIfd = tiff.ImageFileDirectories.ElementAt(minLevel);
            var topOrigin = GetOrigin(topIfd, reader, tiff.IsLittleEndian, tiff.Version);
            (int Width, int Height) topWidthHeight = ((int)topIfd.ImageWidth, (int)topIfd.ImageLength);
            var topResolution = GetResolution(topIfd, reader, tiff.IsLittleEndian, tiff.Version);
            var topExtent = GetExtent(topIfd, reader, tiff.IsLittleEndian, tiff.Version, topOrigin, topResolution,
                topWidthHeight.Width,
                topWidthHeight.Height);

            if (topExtent == null)
            {
                throw new ArgumentException("读取 tiff 的范围失败");
            }

            var extent = new Envelope(topExtent[0], topExtent[2], topExtent[1], topExtent[3]);
            gridSet.Extent = extent;

            // var meterEnvelope = extent.Transform(coordinateSystem, CoordinateReferenceSystem.Get(3857));
            // var pixelSize = meterEnvelope.Width / topWidthHeight.Width;
            // gridSet.PixelSize = pixelSize;

            foreach (var zoomLevel in levels)
            {
                var ifd = tiff.ImageFileDirectories.ElementAt(zoomLevel);
                var resolution =
                    GetCurrentResolution(ifd, reader, tiff.IsLittleEndian, tiff.Version, topResolution,
                        topWidthHeight.Width, topWidthHeight.Height);

                if (gridSet.TileWidth == 0)
                {
                    gridSet.TileWidth = (int)ifd.TileWidth;
                }

                if (gridSet.TileHeight == 0)
                {
                    gridSet.TileHeight = (int)ifd.TileLength;
                }

                var res = resolution[0];
                var scaleDenominator = res * gridSet.MetersPerUnit / gridSet.PixelSize;
                var grid = new Grid(zoomLevel)
                {
                    Name = zoomLevel.ToString(),
                    Resolution = res,
                    ScaleDenominator = scaleDenominator,
                    NumTilesWidth = (int)ifd.NumOfTileWidth,
                    NumTilesHeight = (int)ifd.NumOfTileLength,
                    Width = (int)ifd.ImageWidth,
                    Height = (int)ifd.ImageLength
                };
                // var number = grid.NumTilesWidth * grid.NumTilesHeight;
                gridSet.AppendGrid(grid);
            }

            maxLevel = int.Parse(gridSet.GridLevels.Keys.Max());

            var dataType = topIfd.SampleFormat;
            var compression = topIfd.Compression;
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
                    .Append(Math.Round(grid.Resolution, 8))
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
                            Resolution      {Math.Round(topResolution[0], 8)}, {Math.Round(topResolution[1], 8)}, {Math.Round(topResolution[2], 8)}
                            BoundingBox     {Math.Round(topExtent[0], 4)}, {Math.Round(topExtent[1], 4)}, {Math.Round(topExtent[2], 4)}, {Math.Round(topExtent[3], 4)}
                            EPSG            EPSG:{gridSet.SRID} {coordinateSystem.Name} https://epsg.io/{gridSet.SRID}
                            Images
                            Id  Size        Tile Size    Tile Count     Resolution
                            {sb}
                        """;
            Logger.Value.LogInformation(info);
        }

        (TIFF Tiff, GridSet GridSet, CoordinateSystem CoordinateSystem, int MaxZoom ) tuple = (tiff, gridSet,
            coordinateSystem,
            maxLevel);
        return tuple;
    }

    private static CoordinateSystem GetCoordinateSystem(string path, TIFF tiff, BinaryReader reader)
    {
        // 按文档，第一优先级是从 sidecar file 中获取
        var srsWkt = GetSRSFromSidecarFile(path);
        var coordinateSystem = srsWkt == null ? null : CoordinateSystemFactory.CreateFromWkt(srsWkt);

        if (coordinateSystem != null)
        {
            return coordinateSystem;
        }

        var ifd = tiff.ImageFileDirectories.ElementAt(0);
        var geoKeys = ReadGeoKeyEntries(ifd, reader, tiff.IsLittleEndian, tiff.Version);
        var decoder = new GeoTiffIIOMetadataDecoder(geoKeys);

        var modelType = decoder.GetModelType(); // 1 表示投影坐标系, 2 表示地理经纬系， 3 表示地心(X,Y,Z)坐标系
        if (GeoTiffPCSCodes.ModelTypeProjected == modelType)
        {
            coordinateSystem = CreateProjectedCoordinateReferenceSystem(decoder);
        }
        else if (GeoTiffGCSCodes.ModelTypeGeographic == modelType)
        {
            coordinateSystem = CreateGeographicCoordinateReferenceSystem(decoder);
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
        //     var crsName = citation.ToString();
        //     return CoordinateReferenceSystem.Get(crsName);
        // }

        return coordinateSystem;
    }

    private static double[] GetExtent(ImageFileDirectory ifd, BinaryReader reader, bool isLittleEndian, short version,
        double[] topOrigin, double[] topResolution,
        int topWidth, int topHeight)
    {
        var transformValue = ifd.GetDirectoryEntry(TIFFTag.GEOTIFF_MODELTRANSFORMATIONTAG)
            ?.GetValue(reader, isLittleEndian, version);

        if (transformValue is double[] { Length: 16 } matrix)
        {
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

    private static double[] GetOrigin(ImageFileDirectory idf, BinaryReader reader, bool isLittleEndian, short version)
    {
        var modelTilePointDe = idf.GetDirectoryEntry(TIFFTag.GEOTIFF_MODELTIEPOINTTAG);
        var modelTransformationDe = idf.GetDirectoryEntry(TIFFTag.GEOTIFF_MODELTRANSFORMATIONTAG);

        var modelTilePoint = modelTilePointDe?.GetValue(reader, isLittleEndian, version);
        if (modelTilePoint is double[] { Length: 6 } array)
        {
            return
            [
                array[3],
                array[4],
                array[5]
            ];
        }

        var modelTransformation = modelTransformationDe?.GetValue(reader, isLittleEndian, version);
        if (modelTransformation is double[] transformArr)
        {
            return
            [
                transformArr[3],
                transformArr[7],
                transformArr[11]
            ];
        }

        throw new Exception("The image does not have an affine transformation.");
    }

    private static double[] GetResolution(ImageFileDirectory idf, BinaryReader reader, bool isLittleEndian,
        short version)
    {
        var modelPixelScale = idf.GetDirectoryEntry(TIFFTag.GEOTIFF_MODELPIXELSCALETAG)
            ?.GetValue(reader, isLittleEndian, version);
        var transformation = idf.GetDirectoryEntry(TIFFTag.GEOTIFF_MODELTRANSFORMATIONTAG)
            ?.GetValue(reader, isLittleEndian, version);

        if (modelPixelScale is double[] scaleArr)
        {
            var res = new[]
            {
                scaleArr[0],
                -scaleArr[1],
                scaleArr[2],
            };
            return res;
        }

        if (transformation is not double[] matrix)
        {
            return null;
        }

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

    private static double[] GetCurrentResolution(ImageFileDirectory ifd, BinaryReader reader, bool isLittleEndian,
        short version,
        double[] topResolution, int topWidth, int topHeight)
    {
        var resolution = GetResolution(ifd, reader, isLittleEndian, version);
        if (resolution != null)
        {
            return resolution;
        }

        if (topResolution == null)
        {
            return null;
        }

        var globalResX = topResolution[0];
        var globalResY = topResolution[1];

        var resX = globalResX * topWidth / ifd.ImageWidth;
        var resY = globalResY * topHeight / ifd.ImageLength;

        return [resX, resY, ifd.ImageWidth, ifd.ImageLength];
    }

    internal static ProjectedCoordinateSystem CreateProjectedCoordinateReferenceSystem(
        GeoTiffIIOMetadataDecoder decoder)
    {
        var projCodeValue = decoder.GetGeoKey(GeoTiffPCSCodes.ProjectedCSTypeGeoKey);
        var projCode = projCodeValue == null ? ushort.MinValue : (ushort)Convert.ToUInt16(projCodeValue);

        // TODO: 异常处理
        var linearUnit = CreateLinearUnit(decoder);

        if (projCode == ushort.MinValue || projCode == GTUserDefinedGeoKey)
        {
            return CreateUserDefinedPCS(decoder, linearUnit);
        }

        var pcrs = CoordinateReferenceSystem.Get(projCode) as ProjectedCoordinateSystem;
        if (pcrs == null)
        {
            return null;
        }

        if (linearUnit == null)
        {
            return pcrs;
        }

        return CoordinateSystemFactory.CreateProjectedCoordinateSystem($"EPSG:{projCode}",
            pcrs.GeographicCoordinateSystem,
            pcrs.Projection, linearUnit, pcrs.GetAxis(0), pcrs.GetAxis(1));
    }

    private static void RefineParameters(GeographicCoordinateSystem baseCRS, ProjectionParameterSet parameters)
    {
        // set the remaining parameters.
        var tempDatum = baseCRS.HorizontalDatum;
        var tempEll = tempDatum.Ellipsoid;
        var inverseFlattening = tempEll.InverseFlattening;
        var semiMajorAxis = tempEll.SemiMajorAxis;
        parameters["semi_minor"] = semiMajorAxis * (1 - 1 / inverseFlattening);
        parameters["semi_major"] = semiMajorAxis;
    }

    private static ProjectionParameterSet CreateUserDefinedProjectionParameter(
        String name, GeoTiffIIOMetadataDecoder metadata)
    {
        var coordTrans = metadata.GetGeoKey(GeoTiffPCSCodes.ProjCoordTransGeoKey) as ushort?;

        // throw descriptive exception if ProjCoordTransGeoKey not defined
        if (coordTrans == null || coordTrans == GTUserDefinedGeoKey)
        {
            throw new ArgumentException(
                "CreateUserDefinedProjectionParameter(String name):User defined projections must specify coordinate transformation code in ProjCoordTransGeoKey");
        }

        // getting math transform factory
        return SetParametersForProjection(name, coordTrans.Value, metadata);
    }

    private static ProjectedCoordinateSystem CreateUserDefinedPCS(GeoTiffIIOMetadataDecoder metadata,
        LinearUnit linearUnit)
    {
        var baseCrs = CreateGeographicCoordinateReferenceSystem(metadata);

        var pcsCitationGeoKey = metadata.GetGeoKey(GeoTiffPCSCodes.PCSCitationGeoKey) as string;
        var projectedCrsName = pcsCitationGeoKey == null ? "unnamed" : CleanName(pcsCitationGeoKey);
        var projCode = metadata.GetGeoKey(GeoTiffPCSCodes.ProjectionGeoKey) as ushort?;
        var projUserDefined = projCode == null || GTUserDefinedGeoKey == projCode;

        string projectionName;
        ProjectionParameterSet parameters = null;
        if (projUserDefined)
        {
            var citationName = metadata.GetGeoKey(GeoTiffGCSCodes.GTCitationGeoKey) as string;
            if ((projectedCrsName == null || "unnamed".Equals(projectedCrsName, StringComparison.OrdinalIgnoreCase))
                && citationName != null)
            {
                // Fallback on GTCitation
                projectedCrsName = citationName;
            }

            // Fall back on citation
            projectionName = pcsCitationGeoKey ?? (citationName ?? "unnamed");

            parameters = CreateUserDefinedProjectionParameter(projectionName, metadata);
            RefineParameters(baseCrs, parameters);

            // transform = mtFactory.createParameterizedTransform(parameters);
        }
        else
        {
            var crs = CoordinateReferenceSystem.Get(projCode.Value) as ProjectedCoordinateSystem;
            if (crs == null)
            {
                throw new ArgumentException("无法获取投影坐标系");
            }

            parameters = GetProjectionParameterSet(crs.Projection);
            RefineParameters(baseCrs, parameters);
        }

        var projection = CoordinateSystemFactory.CreateProjection(projectedCrsName, projectedCrsName,
            parameters.ToProjectionParameter().ToList());
        return CoordinateSystemFactory.CreateProjectedCoordinateSystem(projectedCrsName, baseCrs,
            projection,
            linearUnit, null, null);
    }

    private static ProjectionParameterSet GetProjectionParameterSet(IProjection projection)
    {
        var parameters = new List<ProjectionParameter>();
        for (var i = 0; i < projection.NumParameters; ++i)
        {
            var parameter = projection.GetParameter(i);
            parameters.Add(new ProjectionParameter(parameter.Name, parameter.Value));
        }

        return new ProjectionParameterSet(parameters);
    }

    internal static string GetSRSFromSidecarFile(string path)
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

    /// <summary>
    /// 1024 1 表示投影坐标系，2 表示地理经纬系，3 表示地心(X,Y,Z)坐标系
    /// 2048 地理类型代码，指出采用哪一个地理坐标系统。值为 32767 表示为自定义的坐标系。
    /// </summary>
    /// <param name="ifd"></param>
    /// <param name="reader"></param>
    /// <param name="isLittleEndian"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static Dictionary<int, GeoKeyEntry> ReadGeoKeyEntries(ImageFileDirectory ifd, BinaryReader reader,
        bool isLittleEndian,
        short version)
    {
        if (ifd.GetDirectoryEntry(TIFFTag.GEOTIFF_GEOKEYDIRECTORYTAG)
                ?.GetValue(reader, isLittleEndian, version) is not ushort[] geoKeys)
        {
            return new Dictionary<int, GeoKeyEntry>();
        }

        var doubleParamsDe = ifd.GetDirectoryEntry(TIFFTag.GEOTIFF_GEODOUBLEPARAMSTAG);
        var doubleParams = doubleParamsDe?.GetValue(reader, isLittleEndian, version) as double[];

        var geoAsciiParams =
            ifd.GetDirectoryEntry(TIFFTag.GEOTIFF_GEOASCIIPARAMSTAG)
                ?.GetValue(reader, isLittleEndian, version) as string;

        var result = new Dictionary<int, GeoKeyEntry>();
        for (var i = 0; i < geoKeys.Length / 4; i++)
        {
            var a = geoKeys[i * 4];
            var b = geoKeys[i * 4 + 1];
            var c = geoKeys[i * 4 + 2];
            var d = geoKeys[i * 4 + 3];

            var entry = new GeoKeyEntry
            {
                KeyId = a,
                Location = b,
                Count = c,
                ValueOrOffset = d
            };
            switch (entry.Location)
            {
                case 34737:
                {
                    var end = entry.Count - 1 + entry.ValueOrOffset;
                    if (string.IsNullOrEmpty(geoAsciiParams))
                    {
                        throw new ArgumentException("GEOTIFF_GEOASCIIPARAMS is not defined");
                    }

                    var piece = geoAsciiParams[entry.ValueOrOffset..end].Split('=');
                    entry.Value = piece.Length == 1 ? piece[0] : piece[1].Trim();
                    break;
                }
                case 34736:
                {
                    if (doubleParams == null)
                    {
                        throw new ArgumentException("GEOTIFF_GEODOUBLEPARAMS is not defined");
                    }

                    entry.Value = doubleParams[entry.ValueOrOffset];
                    break;
                }
                default:
                {
                    entry.Value = entry.ValueOrOffset;
                    break;
                }
            }

            result.Add(entry.KeyId, entry);
        }

        return result;
    }

    private static ProjectionParameterSet SetParametersForProjection(string name, int coordTransCode,
        GeoTiffIIOMetadataDecoder metadata)
    {
        ProjectionParameterSet parameters = null;

        var code = coordTransCode;
        if (name == null)
        {
            name = "unnamed";
        }

        /** Transverse Mercator */
        if (name.Equals("transverse_mercator", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_TransverseMercator)
        {
            parameters = DefaultParameters.CreateTransverseMercator();
            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["scale_factor"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey, metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }


        /** Equidistant Cylindrical - Plate Caree - Equirectangular */
        if (name.Equals("Equidistant_Cylindrical", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Plate_Carree", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Equidistant_Cylindrical", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_Equirectangular)
        {
            parameters = DefaultParameters.CreateEquidistantCylindrical();
            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["central_meridian"] = GetOriginLong(metadata);

            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Mercator_1SP Mercator_2SP */
        if (name.Equals("mercator_1SP", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Mercator_2SP", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_Mercator)
        {
            var standard_parallel_1 = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel1GeoKey, metadata);
            var isMercator2SP = false;
            if (!double.IsNaN(standard_parallel_1))
            {
                parameters = DefaultParameters.CreateMercator_2SP();
                isMercator2SP = true;
            }
            else parameters = DefaultParameters.CreateMercator_1SP();

            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);
            if (isMercator2SP)
                parameters["standard_parallel_1"] = standard_parallel_1;
            else
                parameters["scale_factor"] = GetScaleFactor(metadata);

            return parameters;
        }

        /** Lambert_conformal_conic_1SP */
        if (name.Equals("lambert_conformal_conic_1SP", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_LambertConfConic_Helmert)
        {
            parameters = DefaultParameters.lambert_conformal_conic_1SP();
            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["scale_factor"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey, metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Lambert_conformal_conic_2SP */
        if (name.Equals("lambert_conformal_conic_2SP", StringComparison.OrdinalIgnoreCase)
            || name.Equals("lambert_conformal_conic_2SP_Belgium", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_LambertConfConic_2SP)
        {
            parameters = DefaultParameters.lambert_conformal_conic_2SP();
            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["standard_parallel_1"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel1GeoKey, metadata);
            parameters["standard_parallel_2"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel2GeoKey, metadata);

            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Krovak */
        if (name.Equals("Krovak", StringComparison.OrdinalIgnoreCase))
        {
            parameters = DefaultParameters.Krovak();
            parameters["latitude_of_center"] = GetOriginLat(metadata);
            parameters["longitude_of_center"] = GetOriginLong(metadata);
            parameters["azimuth"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel1GeoKey, metadata);
            parameters["pseudo_standard_parallel_1"] =
                GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel2GeoKey, metadata);
            parameters["scale_factor"] = GetFalseEasting(metadata);

            return parameters;
        }

        // if (name.Equals("equidistant_conic")
        // || code == GeoTiffMetadata2CRSAdapter.CT_EquidistantConic) {
        // parameters = mtFactory
        // .getDefaultParameters("equidistant_conic");
        // parameters.parameter("central_meridian").setValue(
        // getOriginLong());
        // parameters.parameter("latitude_of_origin").setValue(
        // getOriginLat());
        // parameters
        // .parameter("standard_parallel_1")
        // .setValue(
        // this
        // .getGeoKeyAsDouble(GeoTiffIIOMetadataDecoder.ProjStdParallel1GeoKey));
        // parameters
        // .parameter("standard_parallel_2")
        // .setValue(
        // this
        // .getGeoKeyAsDouble(GeoTiffIIOMetadataDecoder.ProjStdParallel2GeoKey));
        // parameters.parameter("false_easting").setValue(
        // getFalseEasting());
        // parameters.parameter("false_northing").setValue(
        // getFalseNorthing());
        //
        // return parameters;
        // }

        /** STEREOGRAPHIC */
        if (name.Equals("stereographic", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_Stereographic)
        {
            parameters = DefaultParameters.Stereographic();
            parameters["central_meridian"] = GetOriginLong(metadata);

            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["scale_factor"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey, metadata);

            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** POLAR_STEREOGRAPHIC. */
        if (name.Equals("polar_stereographic", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_PolarStereographic)
        {
            parameters = DefaultParameters.polar_stereographic();

            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["scale_factor"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey, metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);
            parameters["central_meridian"] = GetOriginLong(metadata);

            return parameters;
        }

        /** Oblique Stereographic */
        if (name.Equals("oblique_stereographic", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_ObliqueStereographic)
        {
            parameters = DefaultParameters.Oblique_Stereographic();

            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["scale_factor"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey, metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);
            return parameters;
        }

        /** OBLIQUE_MERCATOR. */
        if (name.Equals("oblique_mercator", StringComparison.OrdinalIgnoreCase)
            || name.Equals("hotine_oblique_mercator", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_ObliqueMercator)
        {
            parameters = DefaultParameters.oblique_mercator();
            parameters["scale_factor"] = GetScaleFactor(metadata);
            parameters["azimuth"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjAzimuthAngleGeoKey, metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);
            parameters["longitude_of_center"] = GetOriginLong(metadata);
            parameters["latitude_of_center"] = GetOriginLat(metadata);

            return parameters;
        }

        /** albers_Conic_Equal_Area */
        if (name.Equals("albers_Conic_Equal_Area", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_AlbersEqualArea)
        {
            parameters = DefaultParameters.Albers_Conic_Equal_Area();

            parameters["standard_parallel_1"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel1GeoKey, metadata);
            parameters["standard_parallel_2"] = GetGeoKeyAsDouble(GeoTiffPCSCodes.ProjStdParallel2GeoKey, metadata);
            parameters["longitude_of_center"] = GetOriginLong(metadata);
            parameters["latitude_of_center"] = GetOriginLat(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Orthographic */
        if (name.Equals("Orthographic", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_Orthographic)
        {
            parameters = DefaultParameters.Orthographic();

            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["longitude_of_origin"] = GetOriginLong(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Lambert Azimuthal Equal Area */
        if (name.Equals("Lambert_Azimuthal_Equal_Area", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_LambertAzimEqualArea)
        {
            parameters = DefaultParameters.Lambert_Azimuthal_Equal_Area();

            parameters["longitude_of_center"] = GetOriginLong(metadata);
            parameters["latitude_of_center"] = GetOriginLat(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Azimuthal Equidistant */
        if (name.Equals("Azimuthal_Equidistant", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_AzimuthalEquidistant)
        {
            parameters = DefaultParameters.Azimuthal_Equidistant();
            parameters["latitude_of_center"] = GetOriginLat(metadata);
            parameters["longitude_of_center"] = GetOriginLong(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);
            return parameters;
        }

        /** New Zealand Map Grid */
        if (name.Equals("New_Zealand_Map_Grid", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_NewZealandMapGrid)
        {
            parameters = DefaultParameters.New_Zealand_Map_Grid();

            parameters["latitude_of_origin"] = GetOriginLat(metadata);
            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** World Van der Grinten I */
        if (name.Equals("World_Van_der_Grinten_I", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_VanDerGrinten)
        {
            parameters = DefaultParameters.World_Van_der_Grinten_I();

            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }

        /** Sinusoidal */
        if (name.Equals("Sinusoidal", StringComparison.OrdinalIgnoreCase)
            || code == GeoTiffCoordinateTransformationsCodes.CT_Sinusoidal)
        {
            parameters = DefaultParameters.Sinusoidal();
            parameters["central_meridian"] = GetOriginLong(metadata);
            parameters["false_easting"] = GetFalseEasting(metadata);
            parameters["false_northing"] = GetFalseNorthing(metadata);

            return parameters;
        }


        return parameters;
    }

    private static double GetOriginLong(GeoTiffIIOMetadataDecoder metadata)
    {
        var origin = (((metadata.GetGeoKey(GeoTiffPCSCodes.ProjCenterLongGeoKey) as double? ??
                        metadata.GetGeoKey(GeoTiffPCSCodes.ProjNatOriginLongGeoKey) as double?) ??
                       metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseOriginLongGeoKey) as double?) ??
                      metadata.GetGeoKey(GeoTiffPCSCodes.ProjStraightVertPoleLongGeoKey) as double?) ??
                     metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseNorthingGeoKey) as double?;

        return origin ?? 0.0;
    }

    /**
     * Getting the origin lat with a minimum of tolerance with respect to the parameters name. I saw
     * that often people use the wrong geokey to store the false easting, we cannot be too picky we
     * need to get going pretty smoothly.
     *
     * @param metadata to use for searching the origin latitude.
     * @return double origin latitude.
     */
    private static double GetOriginLat(GeoTiffIIOMetadataDecoder metadata)
    {
        var origin = (metadata.GetGeoKey(GeoTiffPCSCodes.ProjCenterLatGeoKey) as double? ??
                      metadata.GetGeoKey(GeoTiffPCSCodes.ProjNatOriginLatGeoKey) as double?) ??
                     metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseOriginLatGeoKey) as double?;

        return origin ?? 0.0;
    }

    private static double GetScaleFactor(GeoTiffIIOMetadataDecoder metadata)
    {
        var scale = metadata.GetGeoKey(GeoTiffPCSCodes.ProjScaleAtCenterGeoKey) as double? ??
                    metadata.GetGeoKey(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey) as double?;
        return scale ?? 1.0;
    }

    private static double GetGeoKeyAsDouble(int projStdParallel1GeoKey, GeoTiffIIOMetadataDecoder metadata)
    {
        return metadata.GetGeoKey(projStdParallel1GeoKey) as double? ?? double.NaN;
    }

    private static double GetFalseNorthing(GeoTiffIIOMetadataDecoder metadata)
    {
        var northing = metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseNorthingGeoKey) as double? ??
                       metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseOriginNorthingGeoKey) as double?;
        return northing ?? 0.0;
    }

    private static double GetFalseEasting(GeoTiffIIOMetadataDecoder metadata)
    {
        var easting = metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseEastingGeoKey) as double? ??
                      metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseOriginEastingGeoKey) as double?;
        return easting ?? 0.0;
    }

    private static string CleanName(String tiffName)
    {
        // look for strange chars
        // $
        var index = tiffName.LastIndexOf('$');
        if (index != -1) tiffName = tiffName.Substring(index + 1);
        // \n
        index = tiffName.LastIndexOf('\n');
        if (index != -1) tiffName = tiffName.Substring(index + 1);
        // \r
        index = tiffName.LastIndexOf('\r');
        if (index != -1) tiffName = tiffName.Substring(index + 1);
        return tiffName;
    }

    private static LinearUnit CreateLinearUnit(GeoTiffIIOMetadataDecoder decoder)
    {
        var unitCode = decoder.GetGeoKey(GeoTiffPCSCodes.ProjLinearUnitsGeoKey) as ushort?;
        if (unitCode == null)
        {
            return null;
        }

        if (GTUserDefinedGeoKey == unitCode.Value)
        {
            var unitSize = decoder.GetGeoKey(GeoTiffPCSCodes.ProjLinearUnitSizeGeoKey) as double?;
            if (unitSize == null)
            {
                throw new ArgumentException(
                    "GeoTiffMetadata2CRSAdapter::createUnit:Must define unit length when using a user defined unit");
            }

            var sz = unitSize.Value;
            var linearUnit = LinearUnit.Metre;
            linearUnit.MetersPerUnit = sz;
            return linearUnit;
        }

        if (decoder.TryGetUnit(unitCode.Value, out var unit))
        {
            return unit as LinearUnit;
        }

        throw new ArgumentException("Unknown unit code: " + unitCode);
    }

    private static AngularUnit CreateAngularUnit(GeoTiffIIOMetadataDecoder decoder)
    {
        var unitCode = decoder.GetGeoKey(GeoTiffGCSCodes.GeogAngularUnitsGeoKey) as long?;
        if (unitCode == null)
        {
            return AngularUnit.Degrees;
        }

        if (GTUserDefinedGeoKey == unitCode)
        {
            var unitSize = decoder.GetGeoKey(GeoTiffGCSCodes.GeogAngularUnitSizeGeoKey);
            if (unitSize == null)
            {
                throw new ArgumentException(
                    "GeoTiffMetadata2CRSAdapter::createUnit:Must define unit length when using a user defined unit");
            }

            var sz = (double)unitSize;
            var radian = AngularUnit.Radian;
            radian.RadiansPerUnit = sz;
            return radian;
        }

        if (decoder.TryGetUnit(unitCode.Value, out var unit))
        {
            return unit as AngularUnit;
        }

        throw new ArgumentException("Unknown unit code: " + unitCode);
    }

    internal static GeographicCoordinateSystem CreateGeographicCoordinateReferenceSystem(
        GeoTiffIIOMetadataDecoder metadata)
    {
        GeographicCoordinateSystem gcs;
        var geographicTypeValue = metadata.GetGeoKey(GeoTiffGCSCodes.GeographicTypeGeoKey);
        int? tempCode = geographicTypeValue == null ? null : Convert.ToInt32(geographicTypeValue);
        // TODO: 异常处理
        var angularUnit = CreateAngularUnit(metadata) ?? AngularUnit.Degrees;
        var linearUnit = CreateLinearUnit(metadata) ?? LinearUnit.Metre;

        if (tempCode == null || GTUserDefinedGeoKey == tempCode)
        {
            gcs = CreateUserDefinedGCS(metadata, linearUnit, angularUnit);
        }
        else
        {
            gcs = CoordinateReferenceSystem.Get(tempCode.Value) as GeographicCoordinateSystem;
            if (gcs == null)
            {
                throw new ArgumentException($"{tempCode} 不是有效的地理坐标系代码");
            }

            if (angularUnit != null && !angularUnit.EqualParams(gcs.PrimeMeridian.AngularUnit))
            {
                var pm = gcs.PrimeMeridian;
                var primeMeridian =
                    CoordinateSystemFactory.CreatePrimeMeridian(pm.Name, angularUnit, pm.Longitude);
                return CoordinateSystemFactory.CreateGeographicCoordinateSystem("EPSG:" + tempCode,
                    angularUnit
                    , gcs.HorizontalDatum, primeMeridian, null, null);
            }
        }

        return gcs;
    }

    private static GeographicCoordinateSystem CreateUserDefinedGCS(GeoTiffIIOMetadataDecoder metadata,
        LinearUnit linearUnit,
        AngularUnit angularUnit)
    {
        var name = metadata.GetGeoKey(GeoTiffGCSCodes.GeogCitationGeoKey) as string;
        if (name == null)
        {
            name = "unnamed";
        }
        else
        {
            name = CleanName(name);
            var values = name.Split("|");
            if (values.Length >= 1)
            {
                name = values[0];
            }
        }

        var datum = CreateGeodeticDatum(linearUnit, metadata);
        var primeMeridian = CreatePrimeMeridian(metadata, angularUnit);
        return CoordinateSystemFactory.CreateGeographicCoordinateSystem(name, angularUnit, datum,
            primeMeridian, null,
            null);
    }

    private static HorizontalDatum CreateGeodeticDatum(LinearUnit unit, GeoTiffIIOMetadataDecoder metadata)
    {
        var datumCodeValue = metadata.GetGeoKey(GeoTiffGCSCodes.GeogGeodeticDatumGeoKey);
        var datumCode = datumCodeValue as ushort?;
        if (datumCode == null)
        {
            if (datumCodeValue is int datumCode1)
            {
                datumCode = (ushort)datumCode1;
            }
            else
            {
                throw new ArgumentException(
                    "A user defined Geographic Coordinate system must include a predefined datum!");
            }
        }

        HorizontalDatum datum;
        if (GTUserDefinedGeoKey == datumCode)
        {
            // datum name
            var name = metadata.GetGeoKey(GeoTiffGCSCodes.GeogCitationGeoKey) as string;
            if (name == null)
            {
                name = "unnamed";
            }
            else
            {
                var values = name.Split("|");
                if (values.Length >= 2)
                {
                    name = values[1];
                }
            }

            // is it WGS84?
            if (name.Trim().Equals("WGS84", StringComparison.OrdinalIgnoreCase))
            {
                return HorizontalDatum.WGS84;
            }

            var ellipsoid = CreateEllipsoid(unit, metadata);

            // TODO: DatumType.HD_Geocentric?
            datum = CoordinateSystemFactory.CreateHorizontalDatum(name, DatumType.HD_Geocentric, ellipsoid,
                null);
        }
        else
        {
            var code = "EPSG:" + datumCode.ToString();
            datum = metadata.TryGetHorizontalDatum(code, out var h) ? h : null;
        }

        return datum;
    }

    private static PrimeMeridian CreatePrimeMeridian(GeoTiffIIOMetadataDecoder metadata, AngularUnit angularUnit)
    {
        var pmCode = metadata.GetGeoKey(GeoTiffGCSCodes.GeogPrimeMeridianGeoKey) as ushort?;
        if (pmCode == null)
        {
            return PrimeMeridian.Greenwich;
        }

        if (pmCode != GTUserDefinedGeoKey)
        {
            return metadata.TryGetPrimeMeridian("EPSG:" + pmCode, out var pm) ? pm : null;
        }

        var name = metadata.GetGeoKey(GeoTiffGCSCodes.GeogCitationGeoKey) as string;
        if (name == null)
        {
            name = "unnamed";
        }
        else
        {
            var values = name.Split("|");
            if (values.Length >= 4)
            {
                name = values[3];
            }
        }

        var pmValue =
            metadata.GetGeoKey(GeoTiffGCSCodes.GeogPrimeMeridianLongGeoKey) as double?;
        // is it Greenwich?
        return pmValue == null
            ? PrimeMeridian.Greenwich
            : CoordinateSystemFactory.CreatePrimeMeridian(name, angularUnit, pmValue.Value);
    }

    private static Ellipsoid CreateEllipsoid(LinearUnit unit, GeoTiffIIOMetadataDecoder metadata)
    {
        var ellipsoidKey = metadata.GetGeoKey(GeoTiffGCSCodes.GeogEllipsoidGeoKey) as ushort?;

        if (ellipsoidKey == GTUserDefinedGeoKey)
        {
            var name = metadata.GetGeoKey(GeoTiffGCSCodes.GeogCitationGeoKey) as string;
            if (name == null)
            {
                name = "unnamed";
            }
            else
            {
                name = CleanName(name);
                var values = name.Split("|");
                if (values.Length >= 3)
                {
                    name = values[2];
                }
            }

            // is it the default for WGS84?
            if (name.Trim().Equals("WGS84", StringComparison.OrdinalIgnoreCase))
            {
                return Ellipsoid.WGS84;
            }

            var temp1 = metadata.GetGeoKey(GeoTiffGCSCodes.GeogSemiMajorAxisGeoKey) as double?;
            var semiMajorAxis = temp1 ?? double.NaN;
            var inverseFlattening = metadata.GetGeoKey(GeoTiffGCSCodes.GeogInvFlatteningGeoKey) as double?;
            // inverseFlattening = semiMajorAxis / (semiMajorAxis - semiMinorAxis)
            // semiMajorAxis - semiMinorAxis = semiMajorAxis / inverseFlattening
            // semiMinorAxis = semiMajorAxis - semiMajorAxis / inverseFlattening
            double semiMinorAxis;
            if (inverseFlattening.HasValue)
            {
                semiMinorAxis = inverseFlattening.Value == 0
                    ? semiMajorAxis
                    : semiMajorAxis - semiMajorAxis / inverseFlattening.Value;
            }
            else
            {
                var temp3 = metadata.GetGeoKey(GeoTiffGCSCodes.GeogSemiMinorAxisGeoKey) as double?;
                semiMinorAxis = temp3 ?? double.NaN;
            }

            return CoordinateSystemFactory.CreateEllipsoid(name, semiMajorAxis, semiMinorAxis, unit);
        }

        if (ellipsoidKey.HasValue)
        {
            if (metadata.TryGetEllipsoid(ellipsoidKey.Value, out var ellipsoid))
            {
                return ellipsoid;
            }

            throw new ArgumentException($"Ellipsoid key {ellipsoidKey.Value} is not supported");
        }

        throw new ArgumentException("Ellipsoid key not found");
    }

    private static bool IsMaskImage(ImageFileDirectory ifd)
    {
        // Get the NewSubfileType tag
        var type = ifd.NewSubfileType;

        // Check if the third bit (bit 2, 0-indexed) of NewSubfileType is set
        return (type & 4) == 4;
    }
}