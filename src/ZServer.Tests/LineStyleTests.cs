using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class LineStyleTests : BaseTests
    {
        [Fact]
        public void Stroke()
        {
            var data = GetFeatures();

            var style = new LineStyle
            {
                Opacity = Expression<float>.New(1),
                Width = Expression<int>.New(2),
                Color = Expression<string>.New("#3ed53e"),
                LineJoin = Expression<string>.New("Round"),
                Translate = Expression<double[]>.New(new double[] { 1, 1 }),
                DashArray = Expression<float[]>.New(Array.Empty<float>()),
                DashOffset = Expression<float>.New(0),
                LineCap = Expression<string>.New("Round"),
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
                graphicsService.Render(Extent, feature.Geometry, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/Stroke.png", bytes);
        }

        [Fact]
        public void Dash()
        {
            var data = GetFeatures();

            var style = new LineStyle
            {
                Opacity = Expression<float>.New(1),
                Width = Expression<int>.New(2),
                Color = Expression<string>.New("#3ed53e"),
                DashArray = Expression<float[]>.New(new[] { 20, 20f }),
                DashOffset = Expression<float>.New(20),
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
                graphicsService.Render(Extent, feature.Geometry, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/Dash.png", bytes);
        }

        [Fact]
        public void Cap()
        {
            var data = GetFeatures();

            var style = new LineStyle
            {
                Opacity = Expression<float>.New(1),
                Width = Expression<int>.New(2),
                Color = Expression<string>.New("#3ed53e"),
                DashArray = Expression<float[]>.New(new[] { 5, 5f }),
                DashOffset = Expression<float>.New(10),
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
                graphicsService.Render(Extent, feature.Geometry, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/CapDash.png", bytes);
        }
    }
}