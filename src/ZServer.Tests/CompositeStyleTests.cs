using System;
using System.IO;
using Xunit;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class CompositeStyleTests : BaseTests
    {
        [Fact]
        public void StrokeAndFill()
        {
            var data = GetFeatures();

            var style1 = new FillStyle
            {
                Antialias = true,
                Opacity = CSharpExpression<float>.New(0.5f),
                Color = CSharpExpression<string>.New("#3ed53e")
            };
            var style2 = new LineStyle
            {
                Opacity = CSharpExpression<float>.New(1),
                Width = CSharpExpression<int>.New(2),
                Color = CSharpExpression<string>.New("#3ed53e"),
                DashArray = CSharpExpression<float[]>.New(Array.Empty<float>()),
                DashOffset = CSharpExpression<float>.New(0),
                LineCap = CSharpExpression<string>.New("Round"),
                LineJoin = CSharpExpression<string>.New("Round"),
                Translate = CSharpExpression<double[]>.New(new double[] { 1, 1 }),
                GapWidth = CSharpExpression<int>.New(10),
                Offset = CSharpExpression<int>.New(0),
                Blur = CSharpExpression<int>.New(0),
                Gradient = CSharpExpression<int>.New(0)
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

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/StrokeAndFill.png", bytes);
        }

        [Fact]
        public void Hatching()
        {
            var data = GetFeatures();
            Uri.TryCreate("shape://times", UriKind.Absolute, out var uri);
            var style1 = new ResourceFillStyle
            {
                Antialias = true,
                Opacity = CSharpExpression<float>.New(0.5f),
                Color = CSharpExpression<string>.New("#3ed53e"),
                Uri = CSharpExpression<Uri>.New(uri)
            };
            var style2 = new LineStyle
            {
                Opacity = CSharpExpression<float>.New(1),
                Width = CSharpExpression<int>.New(2),
                Color = CSharpExpression<string>.New("#3ed53e"),

                DashArray = CSharpExpression<float[]>.New(Array.Empty<float>()),
                DashOffset = CSharpExpression<float>.New(0),
                LineCap = CSharpExpression<string>.New("Round"),
                LineJoin = CSharpExpression<string>.New("Round"),
                Translate = CSharpExpression<double[]>.New(new double[] { 1, 1 }),
                GapWidth = CSharpExpression<int>.New(10),
                Offset = CSharpExpression<int>.New(0),
                Blur = CSharpExpression<int>.New(0),
                Gradient = CSharpExpression<int>.New(0)
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

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/Hatching.png", bytes);
        }
    }
}