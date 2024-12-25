using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ZMap.TileGrid;

public class GridSet
{
    private const int Decimals = 10;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int SRID { get; set; }

    /// <summary>
    /// 瓦片像素宽度
    /// </summary>
    public int TileWidth { get; set; }

    /// <summary>
    /// 瓦片像素高度
    /// </summary>
    public int TileHeight { get; set; }

    /// <summary>
    /// Whether the y-coordinate tileOrigin is at the top (true) or at the bottom
    /// </summary>
    public bool YBaseToggle { get; set; }

    /// <summary>
    /// By default the coordinates are {x,y}, this flag reverses the output for WMTS getcapabilities
    /// </summary>
    public bool YCoordinateFirst { get; set; }

    public bool ScaleWarning { get; set; }

    public double MetersPerUnit { get; set; }

    public double PixelSize { get; set; }

    public Envelope Extent { get; set; }

    public Dictionary<string, Grid> GridLevels { get; } = new();
    public SortedList<double, Grid> ScaleDenominators { get; } = new();

    // /// <summary>
    // /// 按 Grid 的 Index 顺序存储的 Grid 名称
    // /// </summary>
    // public Dictionary<int, string> GridIndexes { get; } = new();

    /// <summary>
    /// true if the resolutions are preserved and the scaleDenominators calculated
    /// false if the resolutions are calculated based on the scale denominators.
    /// </summary>
    public bool ResolutionsPreserved { get; set; }

    public void AppendGrid(Grid grid)
    {
        ScaleDenominators.Add(grid.ScaleDenominator, grid);
        // GridIndexes.Add(grid.Index, grid.Name);
        GridLevels.Add(grid.Name, grid);
    }

    public TileEnvelope GetEnvelope(string matrixSet, long tileX,
        long tileY)
    {
        if (!GridLevels.TryGetValue(matrixSet, out var grid))
        {
            return null;
        }

        var tileLength = grid.Resolution * TileWidth;
        var tileHeight = grid.Resolution * TileHeight;

        var minX = Extent.MinX + tileX * tileLength;
        var maxX = minX + tileLength;
        double minY, maxY;
        if (YBaseToggle)
        {
            minY = Extent.MinY + tileY * tileHeight;
            maxY = minY + tileHeight;
        }
        else
        {
            maxY = Extent.MaxY - tileY * tileHeight;
            minY = maxY - tileHeight;
        }

        var result = new TileEnvelope
        {
            Extent = new Envelope(minX, maxX, minY, maxY),
            ScaleDenominator = grid.ScaleDenominator
        };
        return result;
    }

    public GridArea GetGridArea(Envelope envelope, Grid grid)
    {
        var p2 = GetTileCoordForXyAndZ(envelope.MinX, envelope.MinY, grid.Name);
        var p1 = GetTileCoordForXyAndZ(envelope.MaxX, envelope.MaxY, grid.Name);

        var range = new GridArea
        {
            Level = grid.Name,
            MinX = p2.X,
            MaxX = p1.X,
            MinY = p1.Y,
            MaxY = p2.Y
        };
        return range;
    }

    public Dictionary<int, GridArea> GetGridAreas(Envelope envelope, string level1, string level2)
    {
        if (!GridLevels.TryGetValue(level1, out var grid1))
        {
            throw new ArgumentException("level1 not found");
        }

        if (!GridLevels.TryGetValue(level2, out var grid2))
        {
            throw new ArgumentException("level2 not found");
        }

        Grid minGrid;
        Grid maxGrid;
        if (grid1.Index > grid2.Index)
        {
            minGrid = grid2;
            maxGrid = grid1;
        }
        else
        {
            minGrid = grid1;
            maxGrid = grid2;
        }

        var result = new Dictionary<int, GridArea>();

        for (var i = minGrid.Index; i <= maxGrid.Index; i++)
        {
            var name = GridLevels[i.ToString()].Name;
            var p1 = GetTileCoordForXyAndZ(envelope.MaxX, envelope.MaxY, name);
            var p2 = GetTileCoordForXyAndZ(envelope.MinX, envelope.MinY, name);
            var range = new GridArea
            {
                Level = i.ToString(),
                MinX = p2.X,
                MaxX = p1.X,
                MinY = p1.Y,
                MaxY = p2.Y
            };

            result.Add(i, range);
        }

        return result;
    }

    public (string Z, int X, int Y) GetTileCoordForXyAndZ(double x, double y, string z,
        bool reverseIntersectionPolicy = false)
    {
        var origin = YBaseToggle
            ? [Extent.MinX, Extent.MinY]
            : new[] { Extent.MinX, Extent.MaxY };

        var grid = GridLevels[z];
        var resolution = grid.Resolution;

        var tileCoordX = (x - origin[0]) / resolution / TileWidth;
        var tileCoordY = (origin[1] - y) / resolution / TileHeight;

        if (reverseIntersectionPolicy)
        {
            tileCoordX = Ceil(tileCoordX, Decimals) - 1;
            tileCoordY = Ceil(tileCoordY, Decimals) - 1;
        }
        else
        {
            tileCoordX = Floor(tileCoordX, Decimals);
            tileCoordY = Floor(tileCoordY, Decimals);
        }

        var fX = (int)tileCoordX;

        var fY = (int)tileCoordY;

        return (z, fX, fY);
    }

    private double Ceil(double n, int decimals)
    {
        return Math.Ceiling(Math.Round(n, decimals));
    }

    private double Floor(double n, int decimals)
    {
        return Math.Floor(Math.Round(n, decimals));
    }

    public (int x, int y) CalcXy(double lng, double lat, int level)
    {
        var x = (lng + 180) / 360;
        var titleX = (int)Math.Floor(x * Math.Pow(2, level));
        var latRad = lat * Math.PI / 180;
        var y = (1 - Math.Log(Math.Truncate(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2;
        var titleY = (int)Math.Floor(y * Math.Pow(2, level));
        return (titleX, titleY);
    }

    /// <summary>
    /// 二分法查找最接近的比例尺
    /// </summary>
    /// <param name="scaleDenominator"></param>
    /// <returns></returns>
    [Obsolete("暂时实现得不对，不要使用")]
    public Grid GetNearestLevelGrid(double scaleDenominator)
    {
        if (ScaleDenominators.Count == 0)
        {
            return null;
        }

        // 如果给定的值小于列表中的第一个值，返回第一个键对应的值
        var firstGrid = ScaleDenominators.First().Value;
        if (scaleDenominator < firstGrid.ScaleDenominator)
        {
            return firstGrid;
        }

        var lastGrid = ScaleDenominators.Last().Value;

        // 如果给定的值大于列表中的最后一个值，返回最后一个键对应的值
        if (scaleDenominator > lastGrid.ScaleDenominator)
        {
            return lastGrid;
        }

        // 使用二分查找找到最接近的值
        var index = ScaleDenominators.IndexOfKey(scaleDenominator);
        if (index != -1)
        {
            // 如果找到了精确的键，返回对应的值
            return ScaleDenominators.GetValueAtIndex(index);
        }

        // 如果没有找到精确的键，找到插入点
        index = ~index;

        // 计算与前后值的差距，选择差距最小的值
        var lowerDiff = scaleDenominator - ScaleDenominators.GetKeyAtIndex(index - 1);
        var upperDiff = ScaleDenominators.GetKeyAtIndex(index) - scaleDenominator;

        return lowerDiff <= upperDiff
            ? ScaleDenominators.GetValueAtIndex(index - 1)
            : ScaleDenominators.GetValueAtIndex(index);
    }

    /// <summary>
    /// 冒泡法找到最接近的比例尺
    /// </summary>
    /// <param name="scaleDenominator"></param>
    /// <returns></returns>
    public Grid GetNearestLevel(double scaleDenominator)
    {
        Grid nearestLevel = null;
        var nearestDistance = double.MaxValue;
        foreach (var kv in GridLevels)
        {
            var grid = kv.Value;
            // 比例尺分母是从大到小排列
            // 如果当前比例尺分母比预期的小，
            var distance = Math.Abs(grid.ScaleDenominator - scaleDenominator);
            if (!(distance < nearestDistance))
            {
                continue;
            }

            nearestDistance = distance;
            nearestLevel = grid;
        }

        return nearestLevel;
    }
}