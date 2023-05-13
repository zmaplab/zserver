using System.IO;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Infrastructure;
using ZMap.Renderer.SkiaSharp.Extensions;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp
{
    public class ResourceFillStyleRenderer : FillStyleRenderer
    {
        private static readonly SKPathEffect Times1 = SKPathEffect.Create2DLine(1,
            SKMatrix.CreateSkew(10, 10).Concat(SKMatrix.CreateRotationDegrees(45)));

        private static readonly SKPathEffect Times2 = SKPathEffect.Create2DLine(1,
            SKMatrix.CreateSkew(10, 10).Concat(SKMatrix.CreateRotationDegrees(135)));

        private static readonly SKPathEffect Times = SKPathEffect.CreateSum(Times1, Times2);

        public ResourceFillStyleRenderer(ResourceFillStyle style) :
            base(style)
        {
        }

        protected override SKPaint CreatePaint()
        {
            var style = (ResourceFillStyle)Style;

            var opacity = style.Opacity.Value ?? 1;
            var color = style.Color?.Value;
            var uri = style.Uri?.Value;
            var antialias = Style.Antialias;

            if (uri == null)
            {
                return base.CreatePaint();
            }

            var paint = Cache.GetOrCreate($"RESOURCE_FILL_STYLE_PAINT_{opacity}{color}{antialias}{uri}", _ =>
            {
                switch (uri.Scheme)
                {
                    case "file":
                    {
                        var path = uri.ToPath();
                        var paint = GetDefaultPaint(color, opacity);
                        if (File.Exists(path))
                        {
                            paint.Shader = SKShader.CreateBitmap(SKBitmap.Decode(path), SKShaderTileMode.Repeat,
                                SKShaderTileMode.Repeat);
                        }

                        return paint;
                    }
                    case "shape":
                    {
                        var paint = GetDefaultPaint(color, opacity);
                        paint.PathEffect = uri.DnsSafeHost switch
                        {
                            "times" => Times,
                            _ => paint.PathEffect
                        };

                        return paint;
                    }
                    default:
                    {
                        return GetDefaultPaint(color, opacity);
                    }
                }
            });

            return paint;
        }

        private SKPaint GetDefaultPaint(string color, float opacity)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = ColorUtilities.GetColor(color, opacity)
            };
            return paint;
        }
    }
}