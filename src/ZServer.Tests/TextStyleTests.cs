using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class TextStyleTests : BaseTests
    {
        [Fact]
        public void Text()
        {
            var data = GetFeatures();

            var style = new TextStyle
            {
                Align = Expression<string>.New("center"),
                Property = Expression<string>.New("name"),
                Font = Expression<string[]>.New(new[] { "宋体" }),
                Size = Expression<int>.New(12),
                Rotate = Expression<float>.New(0),
                Transform = Expression<TextTransform>.New(TextTransform.Uppercase),
                Offset = Expression<float[]>.New(Array.Empty<float>())
            };
            var width = 603 * 2;
            var height = 450 * 2;

            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height, memoryCache);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature, style);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/{GetType().Name}_Label.png", bytes);
        }
    }
}