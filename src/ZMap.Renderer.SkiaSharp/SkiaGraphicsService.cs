using System;
using System.IO;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp.GlContexts;
using ZMap.Source;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class SkiaGraphicsService : IGraphicsService
{
    private static readonly SKPaint BorderPaint;
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private readonly GRContext _grContext;
    private readonly SKSurface _gpuSurface;
    private readonly GlContext _glContext;

    static SkiaGraphicsService()
    {
        BorderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = SKColors.Brown.WithAlpha(byte.MaxValue)
        };
    }

    public string TraceIdentifier { get; set; }
    public int Width { get; }
    public int Height { get; }

    public SkiaGraphicsService(int width, int height, bool useGpu = true)
    {
        // _bitmap.Erase(SKColors.Transparent);

        _glContext = useGpu ? GlContextHelper.Create() : null;

        if (useGpu && _glContext == null)
        {
            throw new InvalidOperationException("Gpu graphics service is not initialized.");
        }

        if (_glContext != null)
        {
            _glContext.MakeCurrent();
            _grContext = GRContext.CreateGl();
            _gpuSurface = SKSurface.Create(_grContext, false, new SKImageInfo(width, height));
            _canvas = _gpuSurface.Canvas;
        }
        else
        {
            _bitmap = new SKBitmap(width, height);
            _canvas = new SKCanvas(_bitmap);
        }

        Width = width;
        Height = height;
    }

    public Stream GetImage(string imageFormat, bool bordered = false)
    {
        if (bordered)
        {
            _canvas.DrawRect(new SKRect(0, 0, Width, Height), BorderPaint);
        }

        _canvas.Flush();

        if (_bitmap != null)
        {
            return _bitmap.Encode(GetImageFormat(imageFormat), 90).AsStream();
        }

        _grContext.Flush();
        using var skImage = _gpuSurface.Snapshot();
        var format = GetImageFormat(imageFormat);
        var skData = skImage.Encode(format, 80);
        return skData.AsStream();
    }

    public void Render(Envelope extent, Envelope geometry, ImageData image, RasterStyle style)
    {
        if (image == null || image.IsEmpty)
        {
            return;
        }

        if (Create(style) is IRasterRender<SKCanvas> renderer)
        {
            renderer.Render(_canvas, geometry, extent, Width, Height, image);
        }
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
        _canvas.Dispose();
        _bitmap?.Dispose();
        _gpuSurface?.Dispose();
        _grContext?.Dispose();
        ((IDisposable)_glContext)?.Dispose();
    }

    private IRenderer Create<TStyle>(TStyle style) where TStyle : Style.Style
    {
        return style switch
        {
            ResourceFillStyle resourceFillStyle => new ResourceFillRenderer(resourceFillStyle),
            FillStyle fillStyle => new FillRenderer(fillStyle),
            LineStyle lineStyle => new LineRenderer(lineStyle),
            TextStyle textStyle => new TextRenderer(textStyle),
            SymbolStyle symbolStyle => new SymbolRenderer(symbolStyle),
            RasterStyle rasterStyle => new RasterRender(rasterStyle),
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