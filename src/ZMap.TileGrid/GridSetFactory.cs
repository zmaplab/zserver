using System;
using NetTopologySuite.Geometries;

namespace ZMap.TileGrid;

public static class GridSetFactory
{
    public const double Epsg4326ToMeters = 6378137.0 * 2.0 * Math.PI / 360.0;
    public const double Epsg3857ToMeters = 1.0D;
    public static readonly Envelope GeographicWorld = new(-180.0, 180.0, -90.0, 90.0);

    public static readonly Envelope ProjectedWorld = new(-20037508.342789244, 20037508.342789244, -20037508.342789244,
        20037508.342789244);

    /// <summary>
    /// DPI 96
    /// 一英寸等于 2.54 厘米、0.0254 米
    /// 一英寸等于 96 像素
    /// 则 1 像素等于 0.0254 / 96 米 = 
    /// </summary>
    public const double DefaultPixelSizeMeter = 0.0254 / 96D;

    public const int DefaultLevels = 22;

    /// <summary>
    /// 中国区域
    /// </summary>
    public static readonly Envelope ChinaGeographicWorld = new(73.62, 134.77, 16.7, 53.56);

    /**
 * Note that you should provide EITHER resolutions or scales. Providing both will cause a
 * precondition violation exception.
 */
    public static GridSet CreateGridSet(
        string name,
        int srs,
        Envelope extent,
        bool alignTopLeft,
        double[] resolutions,
        double[] scaleDenominators,
        double? unitsPerPixel,
        double pixelSize,
        string[] scaleNames,
        int tileWidth,
        int tileHeight,
        bool yCoordinateFirst)
    {
        //Assert.notNull(name, "name is null");
        //Assert.notNull(srs, "srs is null");
        //Assert.notNull(extent, "extent is null");
        //Assert.isTrue(!extent.isNull() && extent.isSane(), "Extent is invalid: " + extent);
        // Assert.isTrue(
        //     resolutions != null || scaleDenoms != null,
        //     "The gridset definition must have either resolutions or scale denominators");
        // Assert.isTrue(
        //     resolutions == null || scaleDenoms == null,
        //     "Only one of resolutions or scaleDenoms should be provided, not both");

        if (scaleDenominators == null && resolutions == null)
        {
            throw new ArgumentException("Only one of resolutions or scaleDenoms should be provided, not both");
        }

        for (var i = 1; resolutions != null && i < resolutions.Length; i++)
        {
            if (resolutions[i] >= resolutions[i - 1])
            {
                throw new ArgumentException(
                    "Each resolution should be lower than it's prior one. Res["
                    + i
                    + "] == "
                    + resolutions[i]
                    + ", Res["
                    + (i - 1)
                    + "] == "
                    + resolutions[i - 1]
                    + ".");
            }
        }

        for (var i = 1; scaleDenominators != null && i < scaleDenominators.Length; i++)
        {
            if (scaleDenominators[i] >= scaleDenominators[i - 1])
            {
                throw new ArgumentException(
                    "Each scale denominator should be lower than it's prior one. Scale["
                    + i
                    + "] == "
                    + scaleDenominators[i]
                    + ", Scale["
                    + (i - 1)
                    + "] == "
                    + scaleDenominators[i - 1]
                    + ".");
            }
        }

        var gridSet = new GridSet
        {
            SRID = srs,
            Name = name,
            TileWidth = tileWidth,
            TileHeight = tileHeight,
            ResolutionsPreserved = resolutions != null,
            PixelSize = pixelSize,
            Extent = extent,
            YBaseToggle = alignTopLeft,
            YCoordinateFirst = yCoordinateFirst
        };

        if (unitsPerPixel == null)
        {
            // 地理坐标系
            if (GeographicWorld.Equals(extent))
            {
                gridSet.MetersPerUnit = Epsg4326ToMeters;
            }
            else if (ProjectedWorld.Equals(extent))
            {
                gridSet.MetersPerUnit = Epsg3857ToMeters;
            }
            else
            {
                if (resolutions == null)
                {
                    // log.warn(
                    //     "GridSet "
                    //     + name
                    //     + " was defined without metersPerUnit, assuming 1m/unit."
                    //     + " All scales will be off if this is incorrect.");
                }
                else
                {
                    // log.warn(
                    //     "GridSet "
                    //     + name
                    //     + " was defined without metersPerUnit. "
                    //     + "Assuming 1m per SRS unit for WMTS scale output.");

                    gridSet.ScaleWarning = true;
                }

                gridSet.MetersPerUnit = 1.0;
            }
        }
        else
        {
            gridSet.MetersPerUnit = unitsPerPixel.Value;
        }

        var length = resolutions?.Length ?? scaleDenominators.Length;

        for (var i = 0; i < length; i++)
        {
            var curGrid = new Grid(i);
            if (scaleDenominators != null)
            {
                curGrid.ScaleDenominator = scaleDenominators[i];
                curGrid.Resolution = pixelSize * (curGrid.ScaleDenominator / gridSet.MetersPerUnit);
            }
            else
            {
                curGrid.Resolution = resolutions[i];
                curGrid.ScaleDenominator =
                    curGrid.Resolution * gridSet.MetersPerUnit / DefaultPixelSizeMeter;
            }

            var mapUnitWidth = tileWidth * curGrid.Resolution;
            var mapUnitHeight = tileHeight * curGrid.Resolution;

            var tilesWide =
                (int)Math.Ceiling((extent.Width - mapUnitWidth * 0.01) / mapUnitWidth);
            var tilesHigh =
                (int)Math.Ceiling((extent.Height - mapUnitHeight * 0.01) / mapUnitHeight);

            curGrid.NumTilesWidth = tilesWide;
            curGrid.NumTilesHeight = tilesHigh;
            curGrid.Name = scaleNames?[i] == null ? i.ToString() : scaleNames[i];

            gridSet.AppendGrid(curGrid);
        }

        return gridSet;
    }

    // public static GridSet CreateGridSet(
    //     string name,
    //     int srs,
    //     Envelope extent,
    //     bool alignTopLeft,
    //     int levels,
    //     double? metersPerUnit,
    //     double pixelSize,
    //     int tileWidth,
    //     int tileHeight,
    //     bool yCoordinateFirst)
    // {
    //     var extentWidth = extent.Width;
    //     var extentHeight = extent.Height;
    //
    //     var resX = extentWidth / tileWidth;
    //     var resY = extentHeight / tileHeight;
    //
    //     int tilesWide, tilesHigh;
    //
    //     if (resX <= resY)
    //     {
    //         // use one tile wide by N tiles high
    //         tilesWide = 1;
    //         tilesHigh = (int)Math.Round(resY / resX);
    //         // previous resY was assuming 1 tile high, recompute with the actual number of tiles
    //         // high
    //         resY /= tilesHigh;
    //     }
    //     else
    //     {
    //         // use one tile high by N tiles wide
    //         tilesHigh = 1;
    //         tilesWide = (int)Math.Round(resX / resY);
    //         // previous resX was assuming 1 tile wide, recompute with the actual number of tiles
    //         // wide
    //         resX /= tilesWide;
    //     }
    //
    //     // the maximum of resX and resY is the one that adjusts better
    //     var res = Math.Max(resX, resY);
    //
    //     var adjustedExtentWidth = tilesWide * tileWidth * res;
    //     var adjustedExtentHeight = tilesHigh * tileHeight * res;
    //
    //     var adjExtent = new Envelope(extent.MinX, extent.MinX + adjustedExtentWidth,
    //         extent.MaxY - adjustedExtentHeight, extent.MinY + adjustedExtentHeight);
    //
    //     var resolutions = new double[levels];
    //     resolutions[0] = res;
    //
    //     for (var i = 1; i < levels; i++)
    //     {
    //         resolutions[i] = resolutions[i - 1] / 2;
    //     }
    //
    //     return CreateGridSet(
    //         name,
    //         srs,
    //         adjExtent,
    //         alignTopLeft,
    //         resolutions,
    //         null,
    //         metersPerUnit,
    //         pixelSize,
    //         null,
    //         tileWidth,
    //         tileHeight,
    //         yCoordinateFirst);
    // }

    public static GridSet CreateGridSet(
        string name,
        int srs,
        Envelope extent,
        bool alignTopLeft,
        int levels,
        double? metersPerUnit,
        double pixelSize,
        int tileWidth,
        int tileHeight,
        bool yCoordinateFirst)
    {
        var resolutions = new double[levels];
        var size = extent.Width / tileWidth;
        for (var i = 0; i < levels; i++)
        {
            resolutions[i] = size / Math.Pow(2, i);
        }

        return CreateGridSet(
            name,
            srs,
            extent,
            alignTopLeft,
            resolutions,
            null,
            metersPerUnit,
            pixelSize,
            null,
            tileWidth,
            tileHeight,
            yCoordinateFirst);
    }
}