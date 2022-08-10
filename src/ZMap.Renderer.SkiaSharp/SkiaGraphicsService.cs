using System;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Source;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class SkiaGraphicsService : IGraphicsService
{
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private readonly IMemoryCache _cache;

    public string MapId { get; }
    public Envelope Envelope { get; }
    public int Width { get; }
    public int Height { get; }

    public SkiaGraphicsService(string mapId, int width, int height, Envelope envelope, IMemoryCache cache)
    {
        _cache = cache;
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);
        Width = width;
        Height = height;
        MapId = mapId;
        Envelope = envelope;
    }

    public byte[] GetImage(string imageFormat)
    {
        _canvas.Flush();
        return _bitmap.Encode(GetImageFormat(imageFormat), 80).ToArray();
    }

    public void Render(RasterStyle style, byte[] image)
    {
        throw new NotImplementedException();
    }

    public void Render(VectorStyle style, Feature feature)
    {
        if (Create(style) is IVectorRenderer<SKCanvas> renderer)
        {
            renderer.Render(_canvas, feature, Envelope, Width, Height);
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
            ResourceFillStyle resourceFillStyle => new ResourceFillStyleRenderer(resourceFillStyle, _cache),
            FillStyle fillStyle => new FillStyleRenderer(fillStyle, _cache),
            LineStyle lineStyle => new LineStyleRenderer(lineStyle, _cache),
            TextStyle textStyle => new TextStyleRenderer(textStyle, _cache),
            SymbolStyle symbolStyle => new SymbolStyleRenderer(symbolStyle, _cache),
            RasterStyle rasterStyle => new RasterStyleRender(rasterStyle, _cache),
            _ => throw new NotSupportedException("不支持的样式")
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