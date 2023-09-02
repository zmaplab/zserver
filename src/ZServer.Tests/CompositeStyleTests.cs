using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class CompositeStyleTests : BaseTests
    {
        [Fact]
        public async Task StrokeAndFill()
        {
            var data = GetFeatures();

            var style1 = new FillStyle
            {
                Antialias = true,
                Opacity = CSharpExpression<float?>.New(0.5f),
                Color = CSharpExpression<string>.New("#3ed53e")
            };
            var style2 = new LineStyle
            {
                Opacity = CSharpExpression<float?>.New(1),
                Width = CSharpExpression<int?>.New(2),
                Color = CSharpExpression<string>.New("#3ed53e"),
                DashArray = CSharpExpression<float[]>.New(Array.Empty<float>()),
                DashOffset = CSharpExpression<float?>.New(0),
                LineCap = CSharpExpression<string>.New("Round"),
                LineJoin = CSharpExpression<string>.New("Round"),
                Translate = CSharpExpression<double[]>.New(new double[] { 1, 1 }),
                GapWidth = CSharpExpression<int?>.New(10),
                Offset = CSharpExpression<int?>.New(0),
                Blur = CSharpExpression<int?>.New(0),
                Gradient = CSharpExpression<int?>.New(0)
            };


            var width = 1024;
            var height = 1024;

            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature.Geometry, style1);
                graphicsService.Render(Extent, feature.Geometry, style2);
            }

            var stream = graphicsService.GetImage("image/png");
            var bytes = await stream.ToArrayAsync();
            await File.WriteAllBytesAsync($"images/StrokeAndFill.png", bytes);
        }

        [Fact]
        public async Task Hatching()
        {
            var data = GetFeatures();
            var style1 = new ResourceFillStyle
            {
                Antialias = true,
                Opacity = CSharpExpression<float?>.New(0.5f),
                Color = CSharpExpression<string>.New("#3ed53e"),
                Uri = CSharpExpression<string>.New("shape://times")
            };
            var style2 = new LineStyle
            {
                Opacity = CSharpExpression<float?>.New(1),
                Width = CSharpExpression<int?>.New(2),
                Color = CSharpExpression<string>.New("#3ed53e"),

                DashArray = CSharpExpression<float[]>.New(Array.Empty<float>()),
                DashOffset = CSharpExpression<float?>.New(0),
                LineCap = CSharpExpression<string>.New("Round"),
                LineJoin = CSharpExpression<string>.New("Round"),
                Translate = CSharpExpression<double[]>.New(new double[] { 1, 1 }),
                GapWidth = CSharpExpression<int?>.New(10),
                Offset = CSharpExpression<int?>.New(0),
                Blur = CSharpExpression<int?>.New(0),
                Gradient = CSharpExpression<int?>.New(0)
            };
            var width = 1024;
            var height = 1024;

            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);
            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature.Geometry, style1);
                graphicsService.Render(Extent, feature.Geometry, style2);
            }

            var stream = graphicsService.GetImage("image/png");
            await File.WriteAllBytesAsync($"images/Hatching.png", await stream.ToArrayAsync());
        }
    }
}