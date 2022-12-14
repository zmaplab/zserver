using SkiaSharp;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp
{
    public class RasterStyleRender : SkiaRenderer, IRasterStyleRender<SKCanvas>
    {
        public RasterStyleRender(RasterStyle style)
        {
        }

        protected override SKPaint CreatePaint()
        {
            return new SKPaint();
        }
    }
}