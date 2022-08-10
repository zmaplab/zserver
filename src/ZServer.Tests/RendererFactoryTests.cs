// using System;
// using System.Threading.Tasks;
// using Xunit;
// using ZMap.Renderer.SkiaSharp;
// using ZMap.Style;
//
// namespace ZServer.Tests
// {
//     public class RendererFactoryTests : BaseTests
//     {
//         [Fact]
//         public void CreateRenderer()
//         {
//             var fill = new FillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e")
//             };
//
//             var line = new LineStyle()
//             {
//                 Opacity = Expression<float>.New(1),
//                 Width = Expression<int>.New(2),
//                 Color = Expression<string>.New("#3ed53e")
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
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//             var resourceFillStyle = new ResourceFillStyle()
//             {
//                 Antialias = true,
//                 Opacity = Expression<float>.New(1),
//                 Color = Expression<string>.New("#3ed53e"),
//                 Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute))
//             };
//             Assert.IsType<FillStyleRenderer>(GetRendererFactory().Create(fill));
//             Assert.IsType<LineStyleRenderer>(GetRendererFactory().Create(line));
//             Assert.IsType<TextStyleRenderer>(GetRendererFactory().Create(textStyle));
//             Assert.IsType<SymbolStyleRenderer>(GetRendererFactory().Create(symbolStyle));
//             Assert.IsType<SymbolStyleRenderer>(GetRendererFactory().Create(symbolStyle));
//             Assert.IsType<ResourceFillStyleRenderer>(GetRendererFactory().Create(resourceFillStyle));
//         }
//
//         [Fact]
//         public void ParallelCreateRenderer()
//         {
//             Parallel.For(0, 10000, _ => { CreateRenderer(); });
//         }
//     }
// }