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
                Color = Expression<string>.New("#3ed53e")
            };

            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
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
                DashArray = Expression<float[]>.New(new[] { 10, 10f }),
                DashOffset = Expression<float>.New(10)
            };

            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
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
                Cap = Expression<string>.New("Round")
            };

            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/CapDash.png", bytes);
        }
    }
}