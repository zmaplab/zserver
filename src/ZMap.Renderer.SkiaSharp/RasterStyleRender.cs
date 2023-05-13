using SkiaSharp;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp
{
    public class RasterStyleRender : SkiaRenderer, IRasterStyleRender<SKCanvas>
    {
        private readonly RasterStyle _rasterStyle;

        public RasterStyleRender(RasterStyle style)
        {
            _rasterStyle = style;
        }

        protected override SKPaint CreatePaint()
        {
            return new SKPaint();
        }

        public override string ToString()
        {
            return $"{_rasterStyle}";
        }
    }
}