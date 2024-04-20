using System;
using System.Linq;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;
using CoordinateTransformUtilities = ZMap.Renderer.SkiaSharp.Utilities.CoordinateTransformUtilities;

namespace ZMap.Renderer.SkiaSharp;

public class TextStyleRenderer(TextStyle style) : SkiaRenderer, ITextStyleRenderer<SKCanvas>
{
    public override void Render(SKCanvas graphics, Geometry geometry, Envelope extent, int width, int height)
    {
        if (style.Label == null)
        {
            return;
        }

        var interiorPoint = geometry.InteriorPoint;

        var interiorCoordinate = new Coordinate(interiorPoint.X, interiorPoint.Y);
        // comments: 不能过滤， 缓存区外的数据也要绘制，如跨边界的文字
        // if (!extent.Contains(interiorCoordinate))
        // {
        //     return;
        // }

        var text = style.Label.Value;

        // if (string.IsNullOrWhiteSpace(_style.Property.Body))
        // {
        //     text = feature[_style.Property.Value];
        // }
        // else
        // {
        //     var func = DynamicCompilationUtilities.GetFunc<string>(_style.Property.Body);
        //     text = func?.Value;
        // }

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var transform = style.Transform.Value;
        var offset = style.Offset.Value;

        switch (transform)
        {
            case TextTransform.Lowercase:
                break;
            case TextTransform.Uppercase:
                text = text.ToUpperInvariant();
                break;
            case TextTransform.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // ReSharper disable once InconsistentNaming
        var centroidSKPoint = CoordinateTransformUtilities.WordToExtent(extent,
            width,
            height, interiorCoordinate);

        var offsetX = centroidSKPoint.X;
        var offsetY = centroidSKPoint.Y;

        if (offset != null)
        {
            offsetX += offset.ElementAtOrDefault(0);
            offsetY += offset.ElementAtOrDefault(1);
        }

        using var paint = CreatePaint();

        var backgroundColor = style.BackgroundColor.Value;
        if (!string.IsNullOrWhiteSpace(backgroundColor))
        {
            var rect = new SKRect();
            paint.MeasureText(text, ref rect);

            using var textPath = paint.GetTextPath(text, offsetX - rect.Width / 2, offsetY);
            // Create a new path for the outlines of the path
            using var outlinePath = new SKPath();
            using var backgroundPaint = CreateBackgroundPaint(backgroundColor);
            backgroundPaint.GetFillPath(textPath, outlinePath);
            graphics.DrawPath(outlinePath, backgroundPaint);
        }

        graphics.DrawText(text, offsetX, offsetY, paint);
    }

    private SKPaint CreateBackgroundPaint(string color)
    {
        var size = style.OutlineSize.Value ?? 2;
        if (size <= 0)
        {
            size = 3;
        }

        return new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = size,
            IsAntialias = true,
            Color = ColorUtilities.GetColor(color)
        };
    }

    protected override SKPaint CreatePaint()
    {
        // TODO: 暂时只取第一个字体
        var fontFamily = style.Font.Value.ElementAtOrDefault(0);
        var size = style.Size.Value ?? 14;
        var rotate = style.Rotate.Value ?? 0;
        var color = style.Color?.Value;
        var align = Enum.TryParse<SKTextAlign>(style.Align?.Value, out var a)
            ? a
            : SKTextAlign.Center;

        return new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = ColorUtilities.GetColor(color),
            TextSize = size,
            TextSkewX = rotate,
            TextAlign = align,
            Typeface = FontUtilities.Get(fontFamily)
        };
    }
}