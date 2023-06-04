using System;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp
{
    public class LineStyleRenderer : SkiaRenderer, ILineStyleRenderer<SKCanvas>
    {
        private readonly LineStyle _style;

        public LineStyleRenderer(LineStyle style)
        {
            _style = style;
        }

        protected override SKPaint CreatePaint()
        {
            var opacity = _style.Opacity.Value ?? 1;
            var width = _style.Width.Value ?? 1;
            var color = _style.Color.Value;
            var dashArray = _style.DashArray.Value ?? Array.Empty<float>();
            var dashOffset = _style.DashOffset.Value ?? 1;
            var lineJoin = _style.LineJoin.Value;
            var cap = _style.LineCap.Value;
            // var translate = _style.Translate.Value;
            // var translateAnchor = _style.TranslateAnchor?.Value;
            // var gapWidth = _style.GapWidth.Value;
            // var offset = _style.Offset.Value;
            var blur = _style.Blur.Value ?? 0;
            // var gradient = _style.Gradient.Value;
            
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

            if (blur > 0)
            {
                paint.ImageFilter = SKImageFilter.CreateBlur(blur, blur);
            }

            return paint;
        }
    }
}