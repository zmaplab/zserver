using System;
using System.IO;
using NetTopologySuite.Geometries;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Style;

namespace ZMap.Renderer.SkiaSharp;

public class RasterRender(RasterStyle style) : SkiaRenderer, IRasterRender<SKCanvas>
{
    protected override SKPaint CreatePaint()
    {
        return new SKPaint();
    }

    public void Render(SKCanvas graphics, Envelope geometry, Envelope extent, int width, int height, byte[] image)
    {
        // var origin = YBaseToggle
        //     ? [Extent.MinX, Extent.MinY]
        //     : new[] { Extent.MinX, Extent.MaxY };
        //
        var points = CoordinateTransformUtility.WordToExtent(extent, width, height,
        [
            new Coordinate(geometry.MinX, geometry.MinY),
            new Coordinate(geometry.MinX, geometry.MaxY),
            new Coordinate(geometry.MaxX, geometry.MinY),
            new Coordinate(geometry.MaxX, geometry.MaxY),
        ]);
        var min = points[1];
        var max = points[2];
        min = new SKPoint((float)Math.Round(min.X), (float)Math.Round(min.Y));
        max = new SKPoint((float)Math.Round(max.X), (float)Math.Round(max.Y));

        var destRect = SKRect.Create((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        using var paint = CreatePaint();

        // var skiaImage = SKImage.FromEncodedData(image);
        var skiaImage = SKBitmap.Decode(image);
        if (skiaImage.Width != width || skiaImage.Height != height)
        {
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Scale((float)width / skiaImage.Width, (float)height / skiaImage.Height);
            canvas.DrawBitmap(skiaImage, 0, 0);
            canvas.Flush();
            skiaImage = bitmap;
        }

        // SKBitmap newBitmap = new SKBitmap(skiaImage.Width, skiaImage.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        // // 遍历每个像素
        // for (int y = 0; y < skiaImage.Height; y++)
        // {
        //     for (int x = 0; x < skiaImage.Width; x++)
        //     {
        //         var color = skiaImage.GetPixel(x, y);
        //
        //         // 判断是否为白色像素（这里简单以RGB值都为255判断，可根据实际需求调整）
        //         if (color.Red == 255 && color.Green == 255 && color.Blue == 255)
        //         {
        //             // 设置为透明
        //             newBitmap.SetPixel(x, y, new SKColor(0, 0, 0, 0));
        //         }
        //         else
        //         {
        //             newBitmap.SetPixel(x, y, color);
        //         }
        //     }
        // }

        using var skiaImage2 = SKImage.FromBitmap(skiaImage);
        graphics.DrawImage(skiaImage2, SKRect.Create(0, 0, width, height), destRect, paint);
    }

    public override string ToString()
    {
        return $"{style}";
    }
}