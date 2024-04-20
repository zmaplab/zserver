using SkiaSharp;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class RasterStyleRender(RasterStyle style) : SkiaRenderer, IRasterStyleRender<SKCanvas>
{
    protected override SKPaint CreatePaint()
    {
        return new SKPaint();
    }

    public override string ToString()
    {
        return $"{style}";
    }
}