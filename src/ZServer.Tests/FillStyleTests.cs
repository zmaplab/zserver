using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests;

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
                Opacity = CSharpExpressionV2.Create<float?>("1"),
                Color = CSharpExpressionV2.Create<string>("#3ed53e")
            };

            using var bitmap = new SKBitmap(256, 256);

            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            var width = 256;
            var height = 256;
            new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService( width, height);

            foreach (var feature in data)
            {
                graphicsService.Render(Extent, feature.Geometry, style);
            }

            graphicsService.GetImage("image/png");
        });
    }

    [Fact]
    public async Task Fill()
    {
        var data = GetFeatures();

        var style = new FillStyle
        {
            Antialias = true,
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e")
        };

        var width = 256;
        var height = 256;
        new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
        var graphicsService =
            new SkiaGraphicsService( width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/Fill.png", await stream.ToArrayAsync());
    }

    [Fact]
    public async Task Opacity()
    {
        var data = GetFeatures();

        var style = new FillStyle
        {
            Antialias = true,
            Opacity = CSharpExpressionV2.Create<float?>("0.5f"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e")
        };

        using var bitmap = new SKBitmap(256, 256);

        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var width = 256;
        var height = 256;
        new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
        var graphicsService =
            new SkiaGraphicsService( width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/Fill.png", await stream.ToArrayAsync());
    }

    [Fact]
    public async Task LocalImageFill()
    {
        var data = GetFeatures();

        var style = new ResourceFillStyle
        {
            Antialias = true,
            Opacity = CSharpExpressionV2.Create<float?>("0.5f"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            Uri = CSharpExpressionV2.Create<string>("file://images/colorblocks1.png")
        };

        var width = 256;
        var height = 256;
        new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
        var graphicsService =
            new SkiaGraphicsService( width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/LocalImageFill.png", await stream.ToArrayAsync());
    }
}