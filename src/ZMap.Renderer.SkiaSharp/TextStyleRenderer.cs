using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source;
using ZMap.Style;
using ZMap.Utilities;
using CoordinateTransformUtilities = ZMap.Renderer.SkiaSharp.Utilities.CoordinateTransformUtilities;

namespace ZMap.Renderer.SkiaSharp
{
    public class TextStyleRenderer : SkiaRenderer, ITextStyleRenderer<SKCanvas>
    {
        private readonly TextStyle _style;
        private readonly IMemoryCache _cache;

        public TextStyleRenderer(TextStyle style, IMemoryCache cache)
        {
            _style = style;
            _cache = cache;
        }

        public override void Render(SKCanvas graphics, Feature feature, Envelope extent, int width, int height)
        {
            if (_style.Property == null)
            {
                return;
            }

            var interiorPoint = feature.Geometry.InteriorPoint;

            var interiorCoordinate = new Coordinate(interiorPoint.X, interiorPoint.Y);
            if (!extent.Contains(interiorCoordinate))
            {
                return;
            }

            string text;

            if (string.IsNullOrWhiteSpace(_style.Property.Body))
            {
                text = feature[_style.Property.Value];
            }
            else
            {
                var func = DynamicCompilationUtilities.GetFunc<string>(_style.Property.Body);
                text = func?.Invoke(feature);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var transform = _style.Transform.Invoke(feature);
            var offset = _style.Offset.Invoke(feature);

            switch (transform)
            {
                case TextTransform.Lowercase:
                    break;
                case TextTransform.Uppercase:
                    text = text.ToUpperInvariant();
                    break;
                case TextTransform.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // ReSharper disable once InconsistentNaming
            var centroidSKPoint = CoordinateTransformUtilities.WordToExtent(extent,
                width,
                height, interiorCoordinate);

            var offsetX = centroidSKPoint.X;
            var offsetY = centroidSKPoint.Y;

            if (offset != null)
            {
                offsetX += offset.ElementAtOrDefault(0);
                offsetY += offset.ElementAtOrDefault(1);
            }

            var paint = CreatePaint(feature);

            var backgroundColor = _style.BackgroundColor.Invoke(feature);
            if (!string.IsNullOrWhiteSpace(backgroundColor))
            {
                var rect = new SKRect();
                paint.MeasureText(text, ref rect);

                using var textPath = paint.GetTextPath(text, offsetX - rect.Width / 2, offsetY);
                // Create a new path for the outlines of the path
                using var outlinePath = new SKPath();
                var framePaint = CreateBackgroundPaint(backgroundColor);
                framePaint.GetFillPath(textPath, outlinePath);

                graphics.DrawPath(outlinePath, framePaint);
            }

            graphics.DrawText(text, offsetX, offsetY, paint);
        }

        private SKPaint CreateBackgroundPaint(string color)
        {
            var key = $"TEXT_STYLE_PAINT_{color}";

            return _cache.GetOrCreate(key, entry =>
            {
                var c = ((string)entry.Key).Replace("TEXT_STYLE_PAINT_", "");
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    StrokeWidth = 2,
                    IsAntialias = true,
                    Color = ColorUtilities.GetColor(c),
                };
                entry.SetValue(paint);
                entry.SetAbsoluteExpiration(TimeSpan.FromDays(1));
                return paint;
            });
        }

        protected override SKPaint CreatePaint(Feature feature)
        {
            var fontFamily = _style.Font?.Invoke(feature);
            var size = _style.Size.Invoke(feature);
            var rotate = _style.Rotate.Invoke(feature);
            var color = _style.Color?.Invoke(feature);
            var align = Enum.TryParse<SKTextAlign>(_style.Align?.Invoke(feature), out var a)
                ? a
                : SKTextAlign.Center;

            var fontKey = fontFamily == null ? string.Empty : string.Join(",", fontFamily);
            var key = $"TEXT_STYLE_PAINT_{fontKey}{size}{rotate}{color}{align}";

            return _cache.GetOrCreate(key, _ => new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = ColorUtilities.GetColor(color),
                TextSize = size,
                TextSkewX = rotate,
                TextAlign = align,
                Typeface = FontUtilities.Get(fontFamily)
            });
        }
    }
}