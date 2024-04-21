using SkiaSharp;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class RasterRender(RasterStyle style) : SkiaRenderer, IRasterRender<SKCanvas>
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