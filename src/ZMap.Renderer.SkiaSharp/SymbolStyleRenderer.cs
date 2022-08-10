using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp.Extensions;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp
{
    public class SymbolStyleRenderer : SkiaRenderer, ISymbolStyleRenderer<SKCanvas>
    {
        private static readonly SKBitmap DefaultImage;
        private readonly SymbolStyle _style;
        private readonly IMemoryCache _cache;

        static SymbolStyleRenderer()
        {
            DefaultImage = SKBitmap.Decode("108.png");
        }

        public SymbolStyleRenderer(SymbolStyle style, IMemoryCache cache)
        {
            _style = style;
            _cache = cache;
        }

        public override void Render(SKCanvas graphics, Feature feature, Envelope extent, int width, int height)
        {
            //   *---    top     ---*
            //   |                  |
            //   left   center    right 
            //   |                  |
            //   *---   bottom   ---*

            var interiorPoint = feature.Geometry.InteriorPoint;
            var centroid = new Coordinate(interiorPoint.X, interiorPoint.Y);

            if (!extent.Contains(centroid))
            {
                return;
            }

            var half = _style.Size.Invoke(feature) / 2;

            var centroidPoint = CoordinateTransformUtilities.WordToExtent(extent,
                width, height, centroid);

            var left = centroidPoint.X - half;
            var top = centroidPoint.Y - half;
            var right = centroidPoint.X + half;
            var bottom = centroidPoint.Y + half;

            // comment: 通过前端 gutter/buffer 计算来处理边界问题

            // if (left < 0)
            // {
            //     right += Math.Abs(left);
            //     left = 0;
            // }
            //
            // if (top < 0)
            // {
            //     bottom += Math.Abs(top);
            //     top = 0;
            // }
            //
            // if (right > width)
            // {
            //     left -= right - width;
            //     right = width;
            // }
            //
            // if (bottom > height)
            // {
            //     top -= bottom - height;
            //     bottom = height;
            // }

            var rect = new SKRect(left, top, right, bottom);

            var image = GetImage(feature);
            graphics.DrawBitmap(image, rect, new SKPaint());
        }

        private SKBitmap GetImage(Feature feature)
        {
            SKBitmap image;
            var uri = _style.Uri?.Invoke(feature);
            if (uri == null)
            {
                image = DefaultImage;
            }
            else
            {
                image = _cache.GetOrCreate($"SYMBOL_STYLE_IMAGE_{_style.Uri.Value}", _ =>
                {
                    switch (uri.Scheme)
                    {
                        case "file":
                        {
                            var path = uri.ToPath();
                            return File.Exists(path) ? SKBitmap.Decode(path) : DefaultImage;
                        }
                        default:
                        {
                            return DefaultImage;
                        }
                    }
                });
            }

            return image;
        }

        protected override SKPaint CreatePaint(Feature feature)
        {
            return null;
        }
    }
}