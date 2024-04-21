using SkiaSharp;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class FillRenderer(FillStyle style) : SkiaRenderer, IFillRenderer<SKCanvas>
{
    protected readonly FillStyle Style = style;

    protected override SKPaint CreatePaint()
    {
        var opacity = Style.Opacity.Value ?? 1;
        var color = Style.Color.Value;
        var antialias = Style.Antialias;

        return new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = antialias,
            Color = ColorUtility.GetColor(color, opacity)
        };
    }
}