using System;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using ZMap.Infrastructure;
using Convert = System.Convert;

namespace ZMap.Renderer.SkiaSharp.Utilities;

public class ColorUtility
{
    private static readonly SKColor DefaultColor = SKColors.Black;
    private static readonly ILogger Logger = Log.CreateLogger<ColorUtility>();

    public static SKColor GetColor(string hexString, float opacity = 1)
    {
        if (string.IsNullOrEmpty(hexString))
        {
            return DefaultColor;
        }

        if (!SKColor.TryParse(hexString, out var color))
        {
            Logger.LogWarning("RGB {IncorrectColorHexString} 不是一个合法的颜色", color);
            return DefaultColor;
        }

        opacity = opacity > 1 ? 1 : opacity;
        var alpha = Convert.ToByte(Math.Round(opacity / 0.0039215, 0));
        return color.WithAlpha(alpha);
    }
}