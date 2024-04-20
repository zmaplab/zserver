using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp.Utilities;

namespace ZServer.Benchmark;
//     BenchmarkDotNet=v0.13.1, OS=macOS Big Sur 11.5.2 (20G95) [Darwin 20.6.0]
//     Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
//     .NET SDK=5.0.302
//     [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
//     DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
//
//
//     |   Method |       Mean |    Error |   StdDev |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
//     |--------- |-----------:|---------:|---------:|-------:|-------:|-------:|----------:|
//     |  NoCache | 2,882.4 ns | 56.61 ns | 79.36 ns | 0.0343 | 0.0191 | 0.0076 |     336 B |
//     | UseCache |   788.4 ns |  2.93 ns |  2.45 ns | 0.0563 |      - |      - |     592 B |
//
// // * Hints *
//     Outliers
//     SKPaintCache.NoCache: Default  -> 5 outliers were removed, 6 outliers were detected (2.70 us, 3.11 us..3.19 us)
//     SKPaintCache.UseCache: Default -> 2 outliers were removed (801.31 ns, 803.78 ns)
//
// // * Legends *
//     Mean      : Arithmetic mean of all measurements
//     Error     : Half of 99.9% confidence interval
//     StdDev    : Standard deviation of all measurements
//     Gen 0     : GC Generation 0 collects per 1000 operations
//     Gen 1     : GC Generation 1 collects per 1000 operations
//     Gen 2     : GC Generation 2 collects per 1000 operations
//     Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
//     1 ns      : 1 Nanosecond (0.000000001 sec)

/// <summary>
/// 通过比较， 还是使用缓存快
/// </summary>
[MemoryDiagnoser]
public class SKPaintCache
{
    private static ConcurrentDictionary<string, SKPaint> PaintCache = new();
        
    float opacity = 1;
    int width = 4;
    string color = "#FFFFFF";
    float[] dashArray = new float[] { 10, 10 };
    int dashOffset = 10;
    string lineJoin = "Butt";
    string cap = "None";
    float[] translate = new float[] { 10, 10 };

    string translateAnchor = "Viewport";
    int gapWidth = 10;
    int offset = 0;
    int blur = 1;
    int gradient = 20;

    [Benchmark]
    public void NoCache()
    {
        using var p = Get();
    }

    [Benchmark]
    public void UseCache()
    {
        var dashArrayKey = dashArray is { Length: 2 } ? $"{dashArray[0]}{dashArray[1]}" : "";
        var key =
            $"{opacity}{width}{color}{color}{dashArrayKey}{dashOffset}{lineJoin}{cap}{translate}{translateAnchor}{gapWidth}{offset}{blur}{gradient}";

        PaintCache.GetOrAdd(key, _ => Get());
    }

    private SKPaint Get()
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = width,
            IsAntialias = true,
            Color = ColorUtilities.GetColor(color, opacity)
        };

        if (Enum.TryParse<SKStrokeCap>(cap, out var strokeCap))
        {
            paint.StrokeCap = strokeCap;
        }

        if (Enum.TryParse<SKStrokeJoin>(lineJoin, out var join))
        {
            paint.StrokeJoin = join;
        }

        if (dashArray is { Length: 2 })
        {
            paint.PathEffect = SKPathEffect.CreateDash(dashArray, dashOffset);
        }

        if (blur is > 0)
        {
            paint.ImageFilter = SKImageFilter.CreateBlur(blur, blur);
        }

        return paint;
    }
}