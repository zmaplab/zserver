using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using ZMap.Source;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp
{
    public class RasterStyleRender : SkiaRenderer, IRasterStyleRender<SKCanvas>
    {
        public RasterStyleRender(RasterStyle style, IMemoryCache cache)
        {
        }

        protected override SKPaint CreatePaint(Feature feature)
        {
            return new SKPaint();
        }
    }
}