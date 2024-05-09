using System.IO;
using System.Threading.Tasks;
using Xunit;
using ZMap;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests;

public class CompositeStyleTests : BaseTests
{
    public float? F()
    {
        return 0.5f;
    }


    [Fact]
    public async Task StrokeAndFill()
    {
        var data = GetFeatures();

        var o = CSharpExpressionV2.Create<float?>("0.5");
        var style1 = new FillStyle
        {
            Antialias = true,
            Opacity = o,
            Color = CSharpExpressionV2.Create<string>("#3ed53e")
        };
        var style2 = new LineStyle
        {
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Width = CSharpExpressionV2.Create<int?>("2"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            DashArray = CSharpExpressionV2.Create<float[]>("default"),
            DashOffset = CSharpExpressionV2.Create<float?>("0"),
            LineCap = CSharpExpressionV2.Create<string>("Round"),
            LineJoin = CSharpExpressionV2.Create<string>("Round"),
            Translate = CSharpExpressionV2.Create<double[]>("new double[] { 1, 1}"),
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
            var s1 = (FillStyle)style1.Clone();
            s1.Accept(Layer.StyleVisitor, feature);
            graphicsService.Render(Extent, feature.Geometry, s1);

            var s2 = (LineStyle)style2.Clone();
            s2.Accept(Layer.StyleVisitor, feature);
            graphicsService.Render(Extent, feature.Geometry, s2);
        }

        var stream = graphicsService.GetImage("image/png");
        var bytes = await stream.ToArrayAsync();
        await File.WriteAllBytesAsync($"images/StrokeAndFill.png", bytes);
    }

    [Fact]
    public async Task Hatching()
    {
        var data = GetFeatures();
        var style1 = new ResourceFillStyle
        {
            Antialias = true,
            Opacity = CSharpExpressionV2.Create<float?>("0.5f"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),
            Uri = CSharpExpressionV2.Create<string>("shape://times")
        };
        var style2 = new LineStyle
        {
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Width = CSharpExpressionV2.Create<int?>("2"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e"),

            DashArray = CSharpExpressionV2.Create<float[]>("default"),
            DashOffset = CSharpExpressionV2.Create<float?>("0"),
            LineCap = CSharpExpressionV2.Create<string>("Round"),
            LineJoin = CSharpExpressionV2.Create<string>("Round"),
            Translate = CSharpExpressionV2.Create<double[]>("new double[] { 1, 1} "),
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
            graphicsService.Render(Extent, feature.Geometry, style1);
            graphicsService.Render(Extent, feature.Geometry, style2);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/Hatching.png", await stream.ToArrayAsync());
    }
}