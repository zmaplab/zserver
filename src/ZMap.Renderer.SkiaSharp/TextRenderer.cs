using System;
using System.Linq;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class TextRenderer(TextStyle style) : SkiaRenderer, ITextRenderer<SKCanvas>
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

        if (string.IsNullOrEmpty(text))
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
        var centroidSKPoint = CoordinateTransformUtility.WordToExtent(extent,
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
        using var font = CreateFont();
        var backgroundColor = style.BackgroundColor.Value;
        if (!string.IsNullOrWhiteSpace(backgroundColor))
        {
            font.MeasureText(text, out var rect, paint);
            var point = new SKPoint(offsetX - rect.Width / 2, offsetY);
            using var textPath = font.GetTextPath(text, point);
            // Create a new path for the outlines of the path
            using var outlinePath = new SKPath();
            using var backgroundPaint = CreateBackgroundPaint(backgroundColor);
            backgroundPaint.GetFillPath(textPath, outlinePath);
            graphics.DrawPath(outlinePath, backgroundPaint);
        }

        var align = Enum.TryParse<SKTextAlign>(style.Align?.Value, out var a)
            ? a
            : SKTextAlign.Center;
        graphics.DrawText(text, offsetX, offsetY, align, font, paint);
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
            Color = ColorUtility.GetColor(color)
        };
    }

    protected override SKPaint CreatePaint()
    {
        // TODO: 暂时只取第一个字体

        var color = style.Color?.Value;

        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = ColorUtility.GetColor(color),
        };

        return paint;
    }

    protected override SKFont CreateFont()
    {
        var fontFamily = style.Font.Value.ElementAtOrDefault(0);
        var size = style.Size.Value ?? 14;
        var rotate = style.Rotate.Value ?? 0;

        var font = FontUtility.Get(fontFamily).ToFont();
        font.Size = size;
        font.SkewX = rotate;
        return font;
    }
}