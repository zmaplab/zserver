using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Infrastructure;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class SymbolRenderer(SymbolStyle style) : SkiaRenderer, ISymbolRenderer<SKCanvas>
{
    private static readonly SKBitmap DefaultImage;

    static SymbolRenderer()
    {
        DefaultImage = SKBitmap.Decode("108.png");
    }

    public override void Render(SKCanvas graphics, Geometry geometry, Envelope extent, int width, int height)
    {
        //   *---    top     ---*
        //   |                  |
        //   left   center    right 
        //   |                  |
        //   *---   bottom   ---*

        var interiorPoint = geometry.InteriorPoint;
        var centroid = new Coordinate(interiorPoint.X, interiorPoint.Y);

        // comment by lewis at 20240522
        // 不能过滤， 有一些越界的矢量也必须绘制
        // if (!extent.Contains(centroid))
        // {
        //     return;
        // }

        var half = (style.Size.Value ?? 14) / 2;

        var centroidPoint = CoordinateTransformUtility.WordToExtent(extent,
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

        var image = GetImage();
        graphics.DrawBitmap(image, rect, new SKPaint());
    }

    private SKBitmap GetImage()
    {
        SKBitmap image;
        var uri = style.Uri.Value;
        if (string.IsNullOrEmpty(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out var u))
        {
            Logger.Value.LogDebug("Invalid image uri: {Uri}", uri);
            image = DefaultImage;
        }
        else
        {
            image = Cache.GetOrCreate($"img_{uri}", entry =>
            {
                SKBitmap i;
                switch (u.Scheme)
                {
                    case "file":
                    {
                        var path = u.GetPath();
                        if (File.Exists(path))
                        {
                            Logger.Value.LogDebug("Load image from file: {Path}", path);
                            i = SKBitmap.Decode(path);
                        }
                        else
                        {
                            Logger.Value.LogDebug("Image file not found: {Path}", path);
                            i = DefaultImage;
                        }

                        break;
                    }
                    default:
                    {
                        Logger.Value.LogDebug("Unsupported image uri scheme: {Scheme}", u.Scheme);
                        i = DefaultImage;
                        break;
                    }
                }

                entry.SetValue(i);
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                return i;
            });
        }

        return image;
    }

    protected override SKPaint CreatePaint()
    {
        return null;
    }
}