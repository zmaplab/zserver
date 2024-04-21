using System;
using System.IO;
using SkiaSharp;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp.Extensions;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class ResourceFillRenderer(ResourceFillStyle style) : FillRenderer(style)
{
    private static readonly SKPathEffect Times1 = SKPathEffect.Create2DLine(1,
        SKMatrix.CreateSkew(10, 10).Concat(SKMatrix.CreateRotationDegrees(45)));

    private static readonly SKPathEffect Times2 = SKPathEffect.Create2DLine(1,
        SKMatrix.CreateSkew(10, 10).Concat(SKMatrix.CreateRotationDegrees(135)));

    private static readonly SKPathEffect Times = SKPathEffect.CreateSum(Times1, Times2);

    protected override SKPaint CreatePaint()
    {
        var style = (ResourceFillStyle)Style;

        var uri = style.Uri.Value;
        if (string.IsNullOrEmpty(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out var u))
        {
            return CreateDefaultPaint();
        }

        SKPaint paint;

        var opacity = style.Opacity.Value ?? 1;
        var color = style.Color?.Value;
        var antialias = Style.Antialias;
        switch (u.Scheme)
        {
            case "file":
            {
                var path = u.ToPath();
                paint = GetDefaultPaint(color, opacity, antialias);
                if (File.Exists(path))
                {
                    paint.Shader = SKShader.CreateBitmap(SKBitmap.Decode(path), SKShaderTileMode.Repeat,
                        SKShaderTileMode.Repeat);
                }

                break;
            }
            case "shape":
            {
                paint = GetDefaultPaint(color, opacity, antialias);
                paint.PathEffect = u.DnsSafeHost switch
                {
                    "times" => Times,
                    _ => paint.PathEffect
                };

                break;
            }
            default:
            {
                paint = GetDefaultPaint(color, opacity, antialias);
                break;
            }
        }

        return paint;
    }

    private SKPaint GetDefaultPaint(string color, float opacity, bool antialias)
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = antialias,
            Color = ColorUtility.GetColor(color, opacity)
        };
        return paint;
    }
}