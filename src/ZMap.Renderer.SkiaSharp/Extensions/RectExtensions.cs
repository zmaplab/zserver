using System;
using SkiaSharp;

namespace ZMap.Renderer.SkiaSharp.Extensions;

public static class RectExtensions
{
    public static SKRect Adjust(this SKRect rect, int width, int height)
    {
        // 若画的图超过边界， 必须移动到可以显示的位置
        // 从左到底部各调一次， 若还有显示不了， 说明图片大小不够， 不需要再处理
        if (rect.Left < 0)
        {
            rect.Right += Math.Abs(rect.Left);
            rect.Left = 0;
        }

        if (rect.Top < 0)
        {
            rect.Bottom += Math.Abs(rect.Top);
            rect.Top = 0;
        }

        if (rect.Right > width)
        {
            rect.Left -= rect.Right - width;
            rect.Right = width;
        }

        if (rect.Bottom > height)
        {
            rect.Top -= rect.Bottom - height;
            rect.Bottom = height;
        }

        return rect;
    }
}