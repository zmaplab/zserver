// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using BenchmarkDotNet.Attributes;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;
// using Microsoft.Extensions.Logging;
// using NetTopologySuite.Features;
// using NetTopologySuite.Geometries;
// using NetTopologySuite.IO;
// using SkiaSharp;
// using ZMap.Renderer;
// using ZMap.Renderer.SkiaSharp;
// using ZMap.Style;
// using Feature = ZMap.Data.Feature;
//
// namespace ZServer.Benchmark
// {
// // BenchmarkDotNet=v0.13.1, OS=macOS Big Sur 11.5.2 (20G95) [Darwin 20.6.0]
// // Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
// // .NET SDK=5.0.302
// //   [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
// //   DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
// //
// //
// // |                          Method |          Mean |        Error |       StdDev |    Gen 0 |    Gen 1 | Allocated |
// // |-------------------------------- |--------------:|-------------:|-------------:|---------:|---------:|----------:|
// // |         DrawFillStyle256OneTime |     816.28 us |    11.060 us |    10.346 us |   5.8594 |        - |     64 KB |
// // |         DrawLineStyle256OneTime |   5,478.41 us |    47.544 us |    39.702 us |   7.8125 |        - |     89 KB |
// // |         DrawTextStyle256OneTime |      65.87 us |     0.526 us |     0.440 us |   4.3945 |        - |     46 KB |
// // |       DrawSymbolStyle256OneTime |   1,781.28 us |    32.804 us |    29.080 us |  15.6250 |   7.8125 |    176 KB |
// // | DrawResourceFillStyle256OneTime |     848.21 us |     5.970 us |     4.661 us |   6.8359 |        - |     74 KB |
// // |          DrawOneStyle256OneTime |     821.50 us |    11.969 us |    10.610 us |   5.8594 |        - |     64 KB |
// // |          DrawOneStyle256TenTime |   7,897.35 us |    74.469 us |    62.185 us |  62.5000 |        - |    639 KB |
// // |          DrawOneStyle512OneTime |   1,473.36 us |    23.316 us |    20.669 us |   5.8594 |        - |     64 KB |
// // |          DrawOneStyle512TenTime |  14,412.22 us |   275.117 us |   257.345 us |  62.5000 |        - |    639 KB |
// // |          DrawAllStyle256OneTime |  10,379.94 us |    84.427 us |    74.843 us |  31.2500 |  15.6250 |    445 KB |
// // |          DrawAllStyle256TenTime | 104,844.42 us | 1,164.248 us | 1,032.076 us | 400.0000 | 200.0000 |  4,467 KB |
// // |          DrawAllStyle512OneTime |  15,982.82 us |   158.525 us |   140.529 us |  31.2500 |        - |    445 KB |
// // |          DrawAllStyle512TenTime | 156,308.93 us | 1,095.861 us |   915.093 us | 250.0000 |        - |  4,408 KB |
// //
// // // * Hints *
// // Outliers
// //   RendererTest.DrawLineStyle256OneTime: Default         -> 2 outliers were removed (5.62 ms, 5.69 ms)
// //   RendererTest.DrawTextStyle256OneTime: Default         -> 2 outliers were removed (68.06 us, 71.27 us)
// //   RendererTest.DrawSymbolStyle256OneTime: Default       -> 1 outlier  was  removed (1.88 ms)
// //   RendererTest.DrawResourceFillStyle256OneTime: Default -> 3 outliers were removed (867.01 us..880.23 us)
// //   RendererTest.DrawOneStyle256OneTime: Default          -> 1 outlier  was  removed (856.37 us)
// //   RendererTest.DrawOneStyle256TenTime: Default          -> 2 outliers were removed (8.24 ms, 8.26 ms)
// //   RendererTest.DrawOneStyle512OneTime: Default          -> 1 outlier  was  removed (1.53 ms)
// //   RendererTest.DrawAllStyle256OneTime: Default          -> 1 outlier  was  removed (10.86 ms)
// //   RendererTest.DrawAllStyle256TenTime: Default          -> 1 outlier  was  removed (108.25 ms)
// //   RendererTest.DrawAllStyle512OneTime: Default          -> 1 outlier  was  removed (16.97 ms)
// //   RendererTest.DrawAllStyle512TenTime: Default          -> 2 outliers were removed (159.18 ms, 161.01 ms)
// //
// // // * Legends *
// //   Mean      : Arithmetic mean of all measurements
// //   Error     : Half of 99.9% confidence interval
// //   StdDev    : Standard deviation of all measurements
// //   Gen 0     : GC Generation 0 collects per 1000 operations
// //   Gen 1     : GC Generation 1 collects per 1000 operations
// //   Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
// //   1 us      : 1 Microsecond (0.000001 sec)
//
//
//     [MemoryDiagnoser]
//     public class RendererTest
//     {
//         private static readonly IServiceProvider ServiceProvider;
//         private static readonly List<Feature> Features;
//         private static readonly Envelope Extent = new(-160.9, 105, -75, 103);
//
//         static RendererTest()
//         {
//             NatashaInitializer.Initialize();
//             //注册+预热组件 , 之后编译会更加快速
//             NatashaInitializer.InitializeAndPreheating();
//
//             var builder = new ConfigurationBuilder();
//             builder.AddJsonFile("appsettings.json");
//
//             var serviceCollection = new ServiceCollection();
//             var configuration = (IConfiguration)builder.Build();
//             serviceCollection.AddMemoryCache();
//             serviceCollection.AddLogging(x => x.AddConsole());
//             serviceCollection.AddZServer(configuration);
//
//             serviceCollection.TryAddSingleton(configuration);
//
//             ServiceProvider = serviceCollection.BuildServiceProvider();
//
//             var json = File.ReadAllText("polygons.json");
//             var reader = new GeoJsonReader();
//             var collection = reader.Read<FeatureCollection>(json);
//
//             Features = new List<Feature>();
//             for (var i = 0; i < 10; ++i)
//             {
//                 Features.AddRange(collection.Select(ToDict).ToList());
//             }
//         }
//
//         [Benchmark]
//         public void DrawFillStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//
//             foreach (var feature in Features)
//             {
//                 render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         ///
//         /// </summary>
//         [Benchmark]
//         public void DrawLineStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//
//             var lineStyle = new LineStyle()
//             {
//                 Opacity = Expression<float>.New(1),
//                 Width = Expression<int>.New(2),
//                 Color = Expression<string>.New("#3ed53e"),
//                 DashArray = Expression<float[]>.New(new[] { 5, 5f }),
//                 DashOffset = Expression<float>.New(10),
//                 Cap = Expression<string>.New("Round")
//             };
//
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render2 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(lineStyle);
//
//             foreach (var feature in Features)
//             {
//                 render2.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         ///
//         /// </summary>
//         [Benchmark]
//         public void DrawTextStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//
//             var textStyle = new TextStyle()
//             {
//                 Align = Expression<string>.New("center"),
//                 Property = Expression<string>.New(null, "feature[\"name\"]"),
//                 Font = Expression<string[]>.New(new[] { "宋体" }),
//                 Size = Expression<int>.New(12),
//                 Rotate = Expression<float>.New(0),
//                 Transform = Expression<TextTransform>.New(TextTransform.Uppercase),
//                 Offset = Expression<float[]>.New(Array.Empty<float>())
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render3 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(textStyle);
//
//             foreach (var feature in Features)
//             {
//                 render3.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         ///
//         /// </summary>
//         [Benchmark]
//         public void DrawSymbolStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//
//             var symbolStyle = new SymbolStyle()
//             {
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
//                 Size = Expression<int>.New(30)
//             };
//
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render4 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(symbolStyle);
//
//             foreach (var feature in Features)
//             {
//                 render4.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         ///
//         /// </summary>
//         [Benchmark]
//         public void DrawResourceFillStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//
//             var resourceFillStyle = new ResourceFillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e"),
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//
//             var render5 =
//                 (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(resourceFillStyle);
//
//             foreach (var feature in Features)
//             {
//                 render5.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         [Benchmark]
//         public void DrawOneStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//
//             foreach (var feature in Features)
//             {
//                 render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         /// 绘制 10 次， bitmap 之前的资源不重复
//         /// </summary>
//         [Benchmark]
//         public void DrawOneStyle256TenTime()
//         {
//             var width = 256;
//             var height = 256;
//
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//
//
//             for (var i = 0; i < 10; ++i)
//             {
//                 foreach (var feature in Features)
//                 {
//                     render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 }
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         [Benchmark]
//         public void DrawOneStyle512OneTime()
//         {
//             var width = 512;
//             var height = 512;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//
//             foreach (var feature in Features)
//             {
//                 render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         /// 绘制 10 次， bitmap 之前的资源不重复
//         /// </summary>
//         [Benchmark]
//         public void DrawOneStyle512TenTime()
//         {
//             var width = 512;
//             var height = 512;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//
//             for (var i = 0; i < 10; ++i)
//             {
//                 foreach (var feature in Features)
//                 {
//                     render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 }
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         ///
//         /// </summary>
//         [Benchmark]
//         public void DrawAllStyle256OneTime()
//         {
//             var width = 256;
//             var height = 256;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//             var lineStyle = new LineStyle()
//             {
//                 Opacity = Expression<float>.New(1),
//                 Width = Expression<int>.New(2),
//                 Color = Expression<string>.New("#3ed53e"),
//                 DashArray = Expression<float[]>.New(new[] { 5, 5f }),
//                 DashOffset = Expression<float>.New(10),
//                 Cap = Expression<string>.New("Round")
//             };
//             var textStyle = new TextStyle()
//             {
//                 Align = Expression<string>.New("center"),
//                 Property = Expression<string>.New(null, "feature[\"name\"]"),
//                 Font = Expression<string[]>.New(new[] { "宋体" }),
//                 Size = Expression<int>.New(12),
//                 Rotate = Expression<float>.New(0),
//                 Transform = Expression<TextTransform>.New(TextTransform.Uppercase),
//                 Offset = Expression<float[]>.New(Array.Empty<float>())
//             };
//             var symbolStyle = new SymbolStyle()
//             {
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
//                 Size = Expression<int>.New(30)
//             };
//             var resourceFillStyle = new ResourceFillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e"),
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//             var render2 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(lineStyle);
//             var render3 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(textStyle);
//             var render4 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(symbolStyle);
//             var render5 =
//                 (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(resourceFillStyle);
//
//             foreach (var feature in Features)
//             {
//                 render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render2.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render3.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render4.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render5.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         /// 绘制 10 次， bitmap 之前的资源不重复
//         /// </summary>
//         [Benchmark]
//         public void DrawAllStyle256TenTime()
//         {
//             var width = 256;
//             var height = 256;
//
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//             var lineStyle = new LineStyle()
//             {
//                 Opacity = Expression<float>.New(1),
//                 Width = Expression<int>.New(2),
//                 Color = Expression<string>.New("#3ed53e"),
//                 DashArray = Expression<float[]>.New(new[] { 5, 5f }),
//                 DashOffset = Expression<float>.New(10),
//                 Cap = Expression<string>.New("Round")
//             };
//             var textStyle = new TextStyle()
//             {
//                 Align = Expression<string>.New("center"),
//                 Property = Expression<string>.New(null, "feature[\"name\"]"),
//                 Font = Expression<string[]>.New(new[] { "宋体" }),
//                 Size = Expression<int>.New(12),
//                 Rotate = Expression<float>.New(0),
//                 Transform = Expression<TextTransform>.New(TextTransform.Uppercase),
//                 Offset = Expression<float[]>.New(Array.Empty<float>())
//             };
//             var symbolStyle = new SymbolStyle()
//             {
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
//                 Size = Expression<int>.New(30)
//             };
//             var resourceFillStyle = new ResourceFillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e"),
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//             var render2 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(lineStyle);
//             var render3 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(textStyle);
//             var render4 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(symbolStyle);
//             var render5 =
//                 (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(resourceFillStyle);
//
//             for (var i = 0; i < 10; ++i)
//             {
//                 foreach (var feature in Features)
//                 {
//                     render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render2.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render3.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render4.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render5.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 }
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         [Benchmark]
//         public void DrawAllStyle512OneTime()
//         {
//             var width = 512;
//             var height = 512;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//             var lineStyle = new LineStyle()
//             {
//                 Opacity = Expression<float>.New(1),
//                 Width = Expression<int>.New(2),
//                 Color = Expression<string>.New("#3ed53e"),
//                 DashArray = Expression<float[]>.New(new[] { 5, 5f }),
//                 DashOffset = Expression<float>.New(10),
//                 Cap = Expression<string>.New("Round")
//             };
//             var textStyle = new TextStyle()
//             {
//                 Align = Expression<string>.New("center"),
//                 Property = Expression<string>.New(null, "feature[\"name\"]"),
//                 Font = Expression<string[]>.New(new[] { "宋体" }),
//                 Size = Expression<int>.New(12),
//                 Rotate = Expression<float>.New(0),
//                 Transform = Expression<TextTransform>.New(TextTransform.Uppercase),
//                 Offset = Expression<float[]>.New(Array.Empty<float>())
//             };
//             var symbolStyle = new SymbolStyle()
//             {
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
//                 Size = Expression<int>.New(30)
//             };
//             var resourceFillStyle = new ResourceFillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e"),
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//             var render2 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(lineStyle);
//             var render3 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(textStyle);
//             var render4 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(symbolStyle);
//             var render5 =
//                 (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(resourceFillStyle);
//
//             foreach (var feature in Features)
//             {
//                 render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render2.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render3.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render4.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 render5.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         /// <summary>
//         /// 绘制 10 次， bitmap 之前的资源不重复
//         /// </summary>
//         [Benchmark]
//         public void DrawAllStyle512TenTime()
//         {
//             var width = 512;
//             var height = 512;
//             var fillStyle = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//             var lineStyle = new LineStyle()
//             {
//                 Opacity = Expression<float>.New(1),
//                 Width = Expression<int>.New(2),
//                 Color = Expression<string>.New("#3ed53e"),
//                 DashArray = Expression<float[]>.New(new[] { 5, 5f }),
//                 DashOffset = Expression<float>.New(10),
//                 Cap = Expression<string>.New("Round")
//             };
//             var textStyle = new TextStyle()
//             {
//                 Align = Expression<string>.New("center"),
//                 Property = Expression<string>.New(null, "feature[\"name\"]"),
//                 Font = Expression<string[]>.New(new[] { "宋体" }),
//                 Size = Expression<int>.New(12),
//                 Rotate = Expression<float>.New(0),
//                 Transform = Expression<TextTransform>.New(TextTransform.Uppercase),
//                 Offset = Expression<float[]>.New(Array.Empty<float>())
//             };
//             var symbolStyle = new SymbolStyle()
//             {
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
//                 Size = Expression<int>.New(30)
//             };
//             var resourceFillStyle = new ResourceFillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e"),
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//
//             using var bitmap = new SKBitmap(width, height);
//             using var canvas = new SKCanvas(bitmap);
//             canvas.Clear();
//
//             var render1 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(fillStyle);
//             var render2 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(lineStyle);
//             var render3 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(textStyle);
//             var render4 = (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(symbolStyle);
//             var render5 =
//                 (IVectorRenderer)ServiceProvider.GetRequiredService<IRendererFactory>().Create(resourceFillStyle);
//
//             for (var i = 0; i < 10; ++i)
//             {
//                 foreach (var feature in Features)
//                 {
//                     render1.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render2.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render3.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render4.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                     render5.PaintAsync(new SkiaSharpRenderContext(bitmap, canvas), feature, Extent, width, height);
//                 }
//             }
//
//             canvas.Flush();
// #if DEBUG
//             var bytes = bitmap.Encode(SKEncodedImageFormat.Png, 80).ToArray();
//             File.WriteAllBytes("fill.png", bytes);
// #endif
//         }
//
//         private static Feature ToDict(IFeature feature)
//         {
//             var dict = new Dictionary<string, object>();
//             foreach (var name in feature.Attributes.GetNames())
//             {
//                 dict.Add(name, feature.Attributes[name]);
//             }
//
//             return new Feature(feature.Geometry, dict);
//         }
//     }
// }