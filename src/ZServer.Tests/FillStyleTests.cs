using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class FillStyleTests : BaseTests
    {
        [Fact]
        public void ParallelFill()
        {
            var data = GetFeatures();

            Parallel.For(0, 10000, _ =>
            {
                var style = new FillStyle
                {
                    Antialias = true,
                    Opacity = Expression<float>.New(1),
                    Color = Expression<string>.New("#3ed53e")
                };

                using var bitmap = new SKBitmap(256, 256);

                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.White);

                var width = 256;
                var height = 256;
                var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
                var graphicsService =
                    new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height, memoryCache);

                foreach (var feature in data)
                {
                    graphicsService.Render(Extent, feature, style);
                }

                var bytes = graphicsService.GetImage("image/png");
            });
        }

        [Fact]
        public void Fill()
        {
            var data = GetFeatures();

            var style = new FillStyle
            {
                Antialias = true,
                Opacity = Expression<float>.New(1),
                Color = Expression<string>.New("#3ed53e")
            };

            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height, memoryCache);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/Fill.png", bytes);
        }

        [Fact]
        public void Opacity()
        {
            var data = GetFeatures();

            var style = new FillStyle
            {
                Antialias = true,
                Opacity = Expression<float>.New(0.5f),
                Color = Expression<string>.New("#3ed53e")
            };

            using var bitmap = new SKBitmap(256, 256);

            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height, memoryCache);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/OpacityFill.png", bytes);
        }

        [Fact]
        public void LocalImageFill()
        {
            var data = GetFeatures();

            Uri.TryCreate("file://images/colorblocks1.png", UriKind.Absolute, out var uri);
            var style = new ResourceFillStyle
            {
                Antialias = true,
                Opacity = Expression<float>.New(0.5f),
                Color = Expression<string>.New("#3ed53e"),
                Uri = Expression<Uri>.New(uri)
            };

            var width = 256;
            var height = 256;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height, memoryCache);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/LocalImageFill.png", bytes);
        }
    }
}