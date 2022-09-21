using System;
using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source;
using ZMap.Style;
using ZMap.Utilities;

namespace ZMap.Renderer.SkiaSharp
{
    public class LineStyleRenderer : SkiaRenderer, ILineStyleRenderer<SKCanvas>
    {
        private readonly LineStyle _style;

        public LineStyleRenderer(LineStyle style)
        {
            _style = style;
        }

        protected override SKPaint CreatePaint(Feature feature)
        {
            var opacity = _style.Opacity.Invoke(feature);
            var width = _style.Width.Invoke(feature);
            var color = _style.Color?.Invoke(feature);
            var dashArray = _style.DashArray?.Invoke(feature);
            var dashOffset = _style.DashOffset?.Invoke(feature) ?? 0;
            var lineJoin = _style.LineJoin?.Invoke(feature);
            var cap = _style.Cap?.Invoke(feature);
            var translate = _style.Translate?.Invoke(feature);
            var translateAnchor = _style.TranslateAnchor?.Invoke(feature);
            var gapWidth = _style.GapWidth?.Invoke(feature);
            var offset = _style.Offset?.Invoke(feature);
            var blur = _style.Blur?.Invoke(feature);
            var gradient = _style.Gradient?.Invoke(feature);

            var dashArrayKey = dashArray is { Length: 2 } ? $"{dashArray[0]}{dashArray[1]}" : "";
            var key =
                $"LINE_STYLE_PAINT_{opacity}{width}{color}{color}{dashArrayKey}{dashOffset}{lineJoin}{cap}{translate}{translateAnchor}{gapWidth}{offset}{blur}{gradient}";

            return Cache.GetOrCreate(key, _ =>
            {
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = width,
                    IsAntialias = true,
                    Color = ColorUtilities.GetColor(color, opacity)
                };

                if (Enum.TryParse<SKStrokeCap>(cap, out var strokeCap))
                {
                    paint.StrokeCap = strokeCap;
                }

                if (Enum.TryParse<SKStrokeJoin>(lineJoin, out var join))
                {
                    paint.StrokeJoin = join;
                }

                if (dashArray is { Length: 2 })
                {
                    paint.PathEffect = SKPathEffect.CreateDash(dashArray, dashOffset);
                }

                if (blur is > 0)
                {
                    paint.ImageFilter = SKImageFilter.CreateBlur(blur.Value, blur.Value);
                }

                return paint;
            });
        }
    }
}