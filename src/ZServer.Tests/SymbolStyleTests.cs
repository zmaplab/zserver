using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class SymbolStyleTests : BaseTests
    {
        [Fact]
        public async Task ImageSymbol()
        {
            var data = GetFeatures();

            var style = new SymbolStyle
            {
                Uri = CSharpExpression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
                Size = CSharpExpression<int?>.New(30)
            };

            var width = 512;
            var height = 512;
            new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature.Geometry, style);
            }


            var stream = graphicsService.GetImage("image/png");
            await File.WriteAllBytesAsync($"images/{GetType().Name}_ImageSymbol.png", await stream.ToArrayAsync());
        }
    }
}