using System;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Infrastructure;
using ZMap.Renderer.SkiaSharp.Utilities;

namespace ZMap.Renderer.SkiaSharp;

public abstract class SkiaRenderer
{
    protected abstract SKPaint CreatePaint();

    protected Lazy<ILogger> Logger { get; }

    protected SkiaRenderer()
    {
        var name = GetType().Name;
        Logger = new Lazy<ILogger>(() => Log.CreateLogger(name));
    }

    protected virtual SKPaint CreateDefaultPaint()
    {
        return new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true,
            Color = SKColors.Black
        };
    }

    public virtual void Render(SKCanvas graphics, Geometry geometry, Envelope extent, int width, int height)
    {
        using var paint = CreatePaint();
        if (paint != null)
        {
            Draw(graphics, paint, geometry, extent, width, height);
        }
    }

    public virtual void Render(SKCanvas graphics, byte[] image, Envelope extent, int width, int height)
    {
    }

    private void DrawPolygon(SKCanvas canvas, SKPaint paint,
        Polygon polygon, Envelope extent, int width, int height)
    {
        if (paint == null)
        {
            return;
        }

        var shellPoints =
            CoordinateTransformUtility.WordToExtent(extent, width, height, polygon.Shell.Coordinates);

        using var path = new SKPath();
        path.FillType = SKPathFillType.EvenOdd;

        path.MoveTo(shellPoints[0]);
        path.AddPoly(shellPoints);

        // 内部的环
        foreach (var polygonHole in polygon.Holes)
        {
            var holePoints =
                CoordinateTransformUtility.WordToExtent(extent, width, height, polygonHole.Coordinates);
            path.MoveTo(holePoints[0]);
            path.AddPoly(holePoints);
        }

        canvas.DrawPath(path, paint);
    }

    private void DrawLineString(SKCanvas canvas, SKPaint paint, LineString lineString, Envelope extent,
        int width, int height)
    {
        if (lineString.IsEmpty)
        {
            return;
        }

        var points = CoordinateTransformUtility.WordToExtent(extent, width, height, lineString.Coordinates);

        // 创建SKPath对象
        using var path = new SKPath();

        var start = points[0];
        path.MoveTo(start.X, start.Y);

        for (var i = 1; i < points.Length; i++)
        {
            path.LineTo(points[i]);
        }

        canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// 递归是不是要展开成 foreach 以提升性能？
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="paint"></param>
    /// <param name="geometry"></param>
    /// <param name="tile"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void Draw(SKCanvas canvas, SKPaint paint, Geometry geometry, Envelope tile, int width,
        int height)
    {
        switch (geometry.OgcGeometryType)
        {
            case OgcGeometryType.LineString:
            {
                DrawLineString(canvas, paint, (LineString)geometry, tile, width, height);
                break;
            }
            case OgcGeometryType.Polygon:
            {
                DrawPolygon(canvas, paint, (Polygon)geometry, tile, width, height);
                break;
            }
            case OgcGeometryType.MultiPolygon:
            case OgcGeometryType.MultiLineString:
            case OgcGeometryType.GeometryCollection:
            case OgcGeometryType.MultiPoint:
            {
                var geometries = (GeometryCollection)geometry;

                foreach (var g in geometries)
                {
                    Draw(canvas, paint, g, tile, width, height);
                }

                break;
            }
            case OgcGeometryType.Point:
                break;
            case OgcGeometryType.CircularString:
                break;
            case OgcGeometryType.CompoundCurve:
                break;
            case OgcGeometryType.CurvePolygon:
                break;
            case OgcGeometryType.MultiCurve:
                break;
            case OgcGeometryType.MultiSurface:
                break;
            case OgcGeometryType.Curve:
                break;
            case OgcGeometryType.Surface:
                break;
            case OgcGeometryType.PolyhedralSurface:
                break;
            case OgcGeometryType.TIN:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}