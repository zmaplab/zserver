using System;
using SkiaSharp;

namespace ZMap.Renderer.SkiaSharp.Utilities
{
    public static class ColorUtilities
    {
        private static readonly SKColor DefaultColor = SKColors.ForestGreen;

        public static SKColor GetColor(string hexString, float opacity = 1)
        {
            if (string.IsNullOrWhiteSpace(hexString))
            {
                return DefaultColor;
            }
            else
            {
                if (!SKColor.TryParse(hexString, out var color))
                {
                    throw new ArgumentException($"RGB {color} 不是一个合法的颜色");
                }
                else
                {
                    opacity = opacity > 1 ? 1 : opacity;
                    var alpha = Convert.ToByte(Math.Round(opacity / 0.0039215, 0));
                    return color.WithAlpha(alpha);
                }
            }
        }
    }
}