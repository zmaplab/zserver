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

    public string MapId { get; }
    public int Width { get; }
    public int Height { get; }

    public SkiaGraphicsService(string mapId, int width, int height)
    {
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);
        Width = width;
        Height = height;
        MapId = mapId;
    }

    public byte[] GetImage(string imageFormat)
    {
        _canvas.Flush();
        return _bitmap.Encode(GetImageFormat(imageFormat), 80).ToArray();
    }

    public void Render(byte[] image, RasterStyle style)
    {
        throw new NotImplementedException();
    }

    public void Render(Envelope extent, Feature feature, VectorStyle style)
    {
        if (Create(style) is IVectorRenderer<SKCanvas> renderer)
        {
            renderer.Render(_canvas, feature, extent, Width, Height);
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