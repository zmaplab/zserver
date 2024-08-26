using System.IO;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests;

public class LineStyleTests : BaseTests
{
    [Fact]
    public async Task Stroke()
    {
        var data = GetFeatures();

        var style = new LineStyle
        {
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Width = CSharpExpressionV2.Create<int?>("2"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            LineJoin = CSharpExpressionV2.Create<string>("Round"),
            Translate = CSharpExpressionV2.Create<double[]>("new double[] { 1, 1 }"),
            DashArray = CSharpExpressionV2.Create<float[]>("default"),
            DashOffset = CSharpExpressionV2.Create<float?>("0"),
            LineCap = CSharpExpressionV2.Create<string>("Round"),
            GapWidth = CSharpExpressionV2.Create<int?>("10"),
            Offset = CSharpExpressionV2.Create<int?>("0"),
            Blur = CSharpExpressionV2.Create<int?>("0"),
            Gradient = CSharpExpressionV2.Create<int?>("0")
        };

        var width = 1024;
        var height = 1024;
        var graphicsService =
            new SkiaGraphicsService(width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/Stroke.png", await stream.ToArrayAsync());
    }

    [Fact]
    public async Task MultiLine()
    {
        var data = GetFeatures("multiline.json");
        var e = data[0].Geometry.EnvelopeInternal;
        var envelope = new Envelope(e.MinX - 0.01, e.MaxX + 0.01, e.MinY - 0.01, e.MaxY + 0.01);
        var style = new LineStyle
        {
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Width = CSharpExpressionV2.Create<int?>("2"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            LineJoin = CSharpExpressionV2.Create<string>("Round"),
            Translate = CSharpExpressionV2.Create<double[]>("new double[] { 1, 1 }"),
            LineCap = CSharpExpressionV2.Create<string>("Round"),
            GapWidth = CSharpExpressionV2.Create<int?>("10"),
            Offset = CSharpExpressionV2.Create<int?>("0"),
            Blur = CSharpExpressionV2.Create<int?>("0"),
            Gradient = CSharpExpressionV2.Create<int?>("0")
        };

        var width = 1024;
        var height = 1024;
        var graphicsService =
            new SkiaGraphicsService(width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(envelope, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/MultiLine.png", await stream.ToArrayAsync());
    }

    [Fact]
    public async Task Dash()
    {
        var data = GetFeatures();

        var style = new LineStyle
        {
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Width = CSharpExpressionV2.Create<int?>("2"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            DashArray = CSharpExpressionV2.Create<float[]>("new[] { 20, 20f }"),
            DashOffset = CSharpExpressionV2.Create<float?>("20"),
            LineCap = CSharpExpressionV2.Create<string>("Round"),
            LineJoin = CSharpExpressionV2.Create<string>("Round"),
            Translate = CSharpExpressionV2.Create<double[]>("new double[] { 1, 1 }"),
            GapWidth = CSharpExpressionV2.Create<int?>("10"),
            Offset = CSharpExpressionV2.Create<int?>("0"),
            Blur = CSharpExpressionV2.Create<int?>("0"),
            Gradient = CSharpExpressionV2.Create<int?>("0")
        };

        var width = 1024;
        var height = 1024;
        var graphicsService =
            new SkiaGraphicsService(width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/Dash.png", await stream.ToArrayAsync());
    }

    [Fact]
    public async Task Cap()
    {
        var data = GetFeatures();

        var style = new LineStyle
        {
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Width = CSharpExpressionV2.Create<int?>("2"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            DashArray = CSharpExpressionV2.Create<float[]>("new[] { 5, 5f }"),
            DashOffset = CSharpExpressionV2.Create<float?>("10"),
            LineCap = CSharpExpressionV2.Create<string>("Round"),
            LineJoin = CSharpExpressionV2.Create<string>("Round"),
            Translate = CSharpExpressionV2.Create<double[]>("new double[] { 1, 1 }"),
            GapWidth = CSharpExpressionV2.Create<int?>("10"),
            Offset = CSharpExpressionV2.Create<int?>("0"),
            Blur = CSharpExpressionV2.Create<int?>("0"),
            Gradient = CSharpExpressionV2.Create<int?>("0")
        };

        var width = 1024;
        var height = 1024;
        var graphicsService =
            new SkiaGraphicsService(width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/CapDash.png", await stream.ToArrayAsync());
    }
}