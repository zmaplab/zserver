using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using ZMap.Extensions;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests;

public class TextStyleTests : BaseTests
{
    [Fact]
    public async Task Text()
    {
        var data = GetFeatures();
        var label = CSharpExpressionV2.Create<string>("name");
        var style = new TextStyle
        {
            Align = CSharpExpressionV2.Create<string>("center"),
            Label = label,
            Font = CSharpExpressionV2.Create<List<string>>("new List<string> { \"宋体\" }"),
            Size = CSharpExpressionV2.Create<int?>("12"),
            Rotate = CSharpExpressionV2.Create<float?>("0"),
            Transform = CSharpExpressionV2.Create<TextTransform>("Uppercase"),
            Offset = CSharpExpressionV2.Create<float[]>(""),
            BackgroundColor = CSharpExpressionV2.Create<string>("#FFFFFF"),
            OutlineSize = CSharpExpressionV2.Create<int?>("1")
        };
        var width = 603 * 2;
        var height = 450 * 2;

        var graphicsService =
            new SkiaGraphicsService(width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(Extent, feature.Geometry, style);
        }

        var stream = graphicsService.GetImage("image/png");
        await File.WriteAllBytesAsync($"images/{GetType().Name}_Label.png", await stream.ToArrayAsync());
    }
}