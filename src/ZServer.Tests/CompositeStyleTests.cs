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
                Opacity = Expression<float>.New(0.5f),
                Color = Expression<string>.New("#3ed53e")
            };
            var style2 = new LineStyle
            {
                Opacity = Expression<float>.New(1),
                Width = Expression<int>.New(2),
                Color = Expression<string>.New("#3ed53e"),
                DashArray = Expression<float[]>.New(Array.Empty<float>()),
                DashOffset = Expression<float>.New(0),
                LineCap = Expression<string>.New("Round"),
                LineJoin = Expression<string>.New("Round"),
                Translate = Expression<double[]>.New(new double[] { 1, 1 }),
                GapWidth = Expression<int>.New(10),
                Offset = Expression<int>.New(0),
                Blur = Expression<int>.New(0),
                Gradient = Expression<int>.New(0)
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
                Opacity = Expression<float>.New(0.5f),
                Color = Expression<string>.New("#3ed53e"),
                Uri = Expression<Uri>.New(uri)
            };
            var style2 = new LineStyle
            {
                Opacity = Expression<float>.New(1),
                Width = Expression<int>.New(2),
                Color = Expression<string>.New("#3ed53e"),

                DashArray = Expression<float[]>.New(Array.Empty<float>()),
                DashOffset = Expression<float>.New(0),
                LineCap = Expression<string>.New("Round"),
                LineJoin = Expression<string>.New("Round"),
                Translate = Expression<double[]>.New(new double[] { 1, 1 }),
                GapWidth = Expression<int>.New(10),
                Offset = Expression<int>.New(0),
                Blur = Expression<int>.New(0),
                Gradient = Expression<int>.New(0)
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