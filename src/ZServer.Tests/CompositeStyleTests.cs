using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
                Color = Expression<string>.New("#3ed53e")
            };


            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style1);
                graphicsService.Render(Extent, feature, style2);
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
                Color = Expression<string>.New("#3ed53e")
            };
            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);
            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style1);
                graphicsService.Render(Extent, feature, style2);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/Hatching.png", bytes);
        }
    }
}