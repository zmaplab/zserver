using System.Linq;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Extensions;

namespace ZMap.Renderer.SkiaSharp.Utilities;

public static class CoordinateTransformUtility
{
    public static SKPoint[] WordToExtent(Envelope extent, int width, int height,
        Coordinate[] coordinates)
    {
        return coordinates
            .Select(coordinate => WordToExtent(extent, width, height, coordinate)).ToArray();
    }

    public static SKPoint WordToExtent(Envelope extent, int width, int height,
        Coordinate coordinate)
    {
        if (coordinate.IsEmpty())
        {
            return SKPoint.Empty;
        }

        var x1 = width * (coordinate.X - extent.MinX) / (extent.MaxX - extent.MinX);
        var y1 = height * (coordinate.Y - extent.MinY) / (extent.MaxY - extent.MinY);
        return new SKPoint((int)x1, height - (int)y1);
    }
}