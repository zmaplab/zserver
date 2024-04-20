using System;
using System.IO;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class SkiaGraphicsService : IGraphicsService
{
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private static readonly SKPaint BorderPaint;

    static SkiaGraphicsService()
    {
        BorderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = SKColors.Gray.WithAlpha(byte.MaxValue)
        };
    }

    public string Identifier { get; }
    public int Width { get; }
    public int Height { get; }

    public SkiaGraphicsService(string identifier, int width, int height)
    {
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);
        Width = width;
        Height = height;
        Identifier = identifier;
    }

    public Stream GetImage(string imageFormat, bool bordered = false)
    {
        if (bordered)
        {
            _canvas.DrawRect(new SKRect(0, 0, Width, Height), BorderPaint);
        }

        _canvas.Flush();
        return _bitmap.Encode(GetImageFormat(imageFormat), 90).AsStream();
    }

    public void Render(byte[] image, RasterStyle style)
    {
        throw new NotImplementedException();
    }

    public void Render(Envelope extent, Geometry geometry, VectorStyle style)
    {
        if (Create(style) is IVectorRenderer<SKCanvas> renderer)
        {
            renderer.Render(_canvas, geometry, extent, Width, Height);
        }
    }

    public void Dispose()
    {
        _bitmap.Dispose();
        _canvas.Dispose();
    }

    private IRenderer Create<TStyle>(TStyle style) where TStyle : Style.Style
    {
        return style switch
        {
            ResourceFillStyle resourceFillStyle => new ResourceFillStyleRenderer(resourceFillStyle),
            FillStyle fillStyle => new FillStyleRenderer(fillStyle),
            LineStyle lineStyle => new LineStyleRenderer(lineStyle),
            TextStyle textStyle => new TextStyleRenderer(textStyle),
            SymbolStyle symbolStyle => new SymbolStyleRenderer(symbolStyle),
            RasterStyle rasterStyle => new RasterStyleRender(rasterStyle),
            _ => throw new NotSupportedException($"不支持的样式: {style.GetType().Name}")
        };
    }

    private SKEncodedImageFormat GetImageFormat(string format)
    {
        return format switch
        {
            "image/png" => SKEncodedImageFormat.Png,
            "image/jpeg" => SKEncodedImageFormat.Jpeg,
            "image/webp" => SKEncodedImageFormat.Webp,
            "image/gif" => SKEncodedImageFormat.Gif,
            "image/bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Png
        };
    }
}