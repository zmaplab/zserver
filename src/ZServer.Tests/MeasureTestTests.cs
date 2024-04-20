using System.IO;
using SkiaSharp;
using Xunit;

namespace ZServer.Tests;

public class MeasureTestTests
{
    [Fact]
    public void MeasureRect()
    {
        var rect1 = SKRect.Empty;
        var rect2 = SKRect.Empty;
        var paint = new SKPaint
        {
            Color = SKColors.Blue,
            TextSize = 12
        };
        var w1 = paint.MeasureText("hello");
        var w2 = paint.MeasureText("hellohello");

        paint.MeasureText("hello", ref rect1);
        paint.MeasureText("hellohello", ref rect2);
        Assert.Equal(w1 * 2, w2);
        using var bitmap = new SKBitmap(256, 256);

        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        canvas.DrawText("hello", 0, 0, paint);
        canvas.DrawText("hellohello", 20, 10, paint);
        canvas.Flush();
        var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
        File.WriteAllBytes($"images/MeasureRect.png", bytes);
    }
}