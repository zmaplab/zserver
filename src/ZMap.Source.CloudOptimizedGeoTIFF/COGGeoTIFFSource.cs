using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using BitMiracle.LibTiff.Classic;
using MessagePack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Projections;
using ProjNet.CoordinateSystems.Transformations;
using ZMap.Infrastructure;
using ZMap.TileGrid;
using static ZMap.Source.CloudOptimizedGeoTIFF.GeoTiffConstants;
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

    private static readonly CoordinateSystemFactory CoordinateSystemFactory = new();
    private static readonly CoordinateTransformationFactory CoordinateTransformationFactory = new();

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
        var coordinateSystem = srsWkt == null ? null : CoordinateSystemFactory.CreateFromWkt(srsWkt);

        if (coordinateSystem != null)
        {
            return coordinateSystem;
        }

        var geoKeys = GeoInfo.Read(tiff);
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
        //     var crsName = citation.Value.ToString();
        //     return CoordinateReferenceSystem.Get(crsName);
        // }

        return coordinateSystem;
    }

    private ProjectedCoordinateSystem CreateProjectedCoordinateReferenceSystem(
        GeoTiffIIOMetadataDecoder decoder)
    {
        var projCode = decoder.GetGeoKey(GeoTiffPCSCodes.ProjectedCSTypeGeoKey) as ushort?;
        if (projCode == null)
        {
            projCode = ushort.MinValue;
        }

        // TODO: 异常处理
        var linearUnit = CreateLinearUnit(decoder);

        if (projCode == ushort.MinValue || projCode == GTUserDefinedGeoKey)
        {
            return CreateUserDefinedPCS(decoder, linearUnit);
        }

        var pcrs = CoordinateReferenceSystem.Get((int)projCode) as ProjectedCoordinateSystem;
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

    private ProjectedCoordinateSystem CreateUserDefinedPCS(GeoTiffIIOMetadataDecoder metadata, LinearUnit linearUnit)
    {
        var baseCrs = CreateGeographicCoordinateReferenceSystem(metadata);

        var pcsCitationGeoKey = metadata.GetGeoKey(GeoTiffPCSCodes.PCSCitationGeoKey) as string;
        var projectedCrsName = pcsCitationGeoKey == null ? "unnamed" : CleanName(pcsCitationGeoKey);
        var projCode = metadata.GetGeoKey(GeoTiffPCSCodes.ProjectionGeoKey) as ushort?;
        bool projUserDefined = projCode == null || GTUserDefinedGeoKey == projCode;

        String projectionName;
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
        return CoordinateSystemFactory.CreateProjectedCoordinateSystem(projectedCrsName, baseCrs, projection,
            linearUnit, null, null);
    }

    private ProjectionParameterSet GetProjectionParameterSet(IProjection projection)
    {
        var parameters = new List<ProjectionParameter>();
        for (var i = 0; i < projection.NumParameters; ++i)
        {
            var parameter = projection.GetParameter(i);
            parameters.Add(new ProjectionParameter(parameter.Name, parameter.Value));
        }

        return new ProjectionParameterSet(parameters);
    }


    private void RefineParameters(GeographicCoordinateSystem baseCRS, ProjectionParameterSet parameters)
    {
        // set the remaining parameters.
        var tempDatum = baseCRS.HorizontalDatum;
        var tempEll = tempDatum.Ellipsoid;
        var inverseFlattening = tempEll.InverseFlattening;
        var semiMajorAxis = tempEll.SemiMajorAxis;
        parameters["semi_minor"] = semiMajorAxis * (1 - 1 / inverseFlattening);
        parameters["semi_major"] = semiMajorAxis;
    }

    private ProjectionParameterSet CreateUserDefinedProjectionParameter(
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

    private ProjectionParameterSet SetParametersForProjection(string name, int coordTransCode,
        GeoTiffIIOMetadataDecoder metadata)
    {
        ProjectionParameterSet parameters = null;

        int code = coordTransCode;
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
            bool isMercator2SP = false;
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

    private double GetScaleFactor(GeoTiffIIOMetadataDecoder metadata)
    {
        var scale = metadata.GetGeoKey(GeoTiffPCSCodes.ProjScaleAtCenterGeoKey) as double? ??
                    metadata.GetGeoKey(GeoTiffPCSCodes.ProjScaleAtNatOriginGeoKey) as double?;
        return scale ?? 1.0;
    }

    private double GetGeoKeyAsDouble(int projStdParallel1GeoKey, GeoTiffIIOMetadataDecoder metadata)
    {
        return metadata.GetGeoKey(projStdParallel1GeoKey) as double? ?? double.NaN;
    }

    private double GetFalseNorthing(GeoTiffIIOMetadataDecoder metadata)
    {
        var northing = metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseNorthingGeoKey) as double? ??
                       metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseOriginNorthingGeoKey) as double?;
        return northing ?? 0.0;
    }

    private double GetFalseEasting(GeoTiffIIOMetadataDecoder metadata)
    {
        var easting = metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseEastingGeoKey) as double? ??
                      metadata.GetGeoKey(GeoTiffPCSCodes.ProjFalseOriginEastingGeoKey) as double?;
        return easting ?? 0.0;
    }

    private string CleanName(String tiffName)
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

    private LinearUnit CreateLinearUnit(GeoTiffIIOMetadataDecoder decoder)
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

        if (decoder.TryGetUnit(unitCode.Value, out IUnit unit))
        {
            return unit as LinearUnit;
        }

        throw new ArgumentException("Unknown unit code: " + unitCode);
    }

    private AngularUnit CreateAngularUnit(GeoTiffIIOMetadataDecoder decoder)
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

        if (decoder.TryGetUnit(unitCode.Value, out IUnit unit))
        {
            return unit as AngularUnit;
        }

        throw new ArgumentException("Unknown unit code: " + unitCode);
    }


    private GeographicCoordinateSystem CreateGeographicCoordinateReferenceSystem(GeoTiffIIOMetadataDecoder metadata)
    {
        GeographicCoordinateSystem gcs;
        var tempCode = metadata.GetGeoKey(GeoTiffGCSCodes.GeographicTypeGeoKey) as ushort?;
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
                var primeMeridian = CoordinateSystemFactory.CreatePrimeMeridian(pm.Name, angularUnit, pm.Longitude);
                return CoordinateSystemFactory.CreateGeographicCoordinateSystem("EPSG:" + tempCode, angularUnit
                    , gcs.HorizontalDatum, primeMeridian, null, null);
            }
        }

        return gcs;
    }

    private GeographicCoordinateSystem CreateUserDefinedGCS(GeoTiffIIOMetadataDecoder metadata, LinearUnit linearUnit,
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
        return CoordinateSystemFactory.CreateGeographicCoordinateSystem(name, angularUnit, datum, primeMeridian, null,
            null);
    }

    private HorizontalDatum CreateGeodeticDatum(LinearUnit unit, GeoTiffIIOMetadataDecoder metadata)
    {
        var datumCode = metadata.GetGeoKey(GeoTiffGCSCodes.GeogGeodeticDatumGeoKey) as ushort?;
        if (datumCode == null)
        {
            throw new ArgumentException("A user defined Geographic Coordinate system must include a predefined datum!");
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
            datum = CoordinateSystemFactory.CreateHorizontalDatum(name, DatumType.HD_Geocentric, ellipsoid, null);
        }
        else
        {
            var code = "EPSG:" + datumCode.ToString();
            datum = metadata.TryGetHorizontalDatum(code, out var h) ? h : null;
        }

        return datum;
    }

    private PrimeMeridian CreatePrimeMeridian(GeoTiffIIOMetadataDecoder metadata, AngularUnit angularUnit)
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

    private Ellipsoid CreateEllipsoid(LinearUnit unit, GeoTiffIIOMetadataDecoder metadata)
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