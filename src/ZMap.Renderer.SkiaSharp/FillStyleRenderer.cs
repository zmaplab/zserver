using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source;
using ZMap.Style;
using ZMap.Utilities;

namespace ZMap.Renderer.SkiaSharp
{
    public class FillStyleRenderer : SkiaRenderer, IFillStyleRenderer<SKCanvas>
    {
        protected readonly FillStyle Style;

        public FillStyleRenderer(FillStyle style)
        {
            Style = style;
        }

        protected override SKPaint CreatePaint(Feature feature)
        {
            var opacity = Style.Opacity.Invoke(feature);
            var color = Style.Color?.Invoke(feature);
            var antialias = Style.Antialias;

            var key = $"FILL_STYLE_PAINT_{opacity}{color}{antialias}";

            // TODO: 是否不设置过期时间就是永久的还是一直都创建?
            return Cache.GetOrCreate(key, _ => new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = antialias,
                Color = ColorUtilities.GetColor(color, opacity)
            });
        }
    }
}