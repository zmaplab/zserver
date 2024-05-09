using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using ZMap;
using ZMap.Style;
using ZServer.Store;

namespace ZServer.Tests;

public class StyleGroupStoreTests : BaseTests
{
    [Fact]
    public async Task LoadFromJson()
    {
        Enum.TryParse("Scale", out ZoomUnits scale);
        Enum.TryParse("Matrix", out ZoomUnits matrix);
        Assert.Equal(ZoomUnits.Scale, scale);
        Assert.Equal(ZoomUnits.Matrix, matrix);
        var json = JsonConvert.DeserializeObject(await File.ReadAllTextAsync("layers.json")) as JObject;
        var store = new StyleGroupStore();
        await store.Refresh(new List<JObject> { json });

        await Test(store);
    }

    [Fact]
    public async Task FindByName()
    {
        var store = GetScopedService<IStyleGroupStore>();
        await Test(store);
    }

    private async Task Test(IStyleGroupStore store)
    {
        var styleGroup = await store.FindAsync("DI_规划道路中线");

        Assert.NotNull(styleGroup);
        Assert.Equal("DI_规划道路中线", styleGroup.Name);
        Assert.Equal("DI_规划道路中线 description", styleGroup.Description);
        Assert.Equal(500, styleGroup.MinZoom);
        Assert.Equal(128000, styleGroup.MaxZoom);
        Assert.Equal(ZoomUnits.Scale, styleGroup.ZoomUnit);
        Assert.NotNull(styleGroup.Styles);

        Assert.Equal(3, styleGroup.Styles.Count);
        Assert.True(styleGroup.Styles[0] is SpriteFillStyle);
        Assert.True(styleGroup.Styles[1] is LineStyle);
        Assert.True(styleGroup.Styles[2] is TextStyle);
        var fillStyle = (SpriteFillStyle)styleGroup.Styles[0];
        var lineStyle = (LineStyle)styleGroup.Styles[1];
        var textStyle = (TextStyle)styleGroup.Styles[2];

        var feature = new Feature(new Point(1, 1), new Dictionary<string, dynamic>
        {
            { "height", 11 },
            { "color", "#3ed53e" },
            { "translateAnchor", "Viewport" },
            { "gapWidth", 1 },
            { "offset", 2 },
            { "blur", 3 },
            { "gradient", 4 },
            {"sprite-pattern","sprite pattern"}
            
        });
        fillStyle.Accept(new ZMapStyleVisitor(), feature);
        lineStyle.Accept(new ZMapStyleVisitor(), feature);
        textStyle.Accept(new ZMapStyleVisitor(), feature);


        Assert.Equal(true, fillStyle.Filter.Value);

        var b = lineStyle.Filter is
        {
            Value: not null
        };
        Assert.True(b);

        CSharpExpressionV2<bool?> c = null;
        Assert.False(c is
        {
            Value: not null
        });

        Assert.False(textStyle.Filter.Value);
        Assert.True(fillStyle.Antialias);
        Assert.Equal(1, fillStyle.Opacity.Value);
        //Assert.Equal("feature[\"opacity\"]", fillStyle.Opacity.Value);
        Assert.Equal("#3ed53e", fillStyle.Color.Value);
        //Assert.Equal("feature[\"color\"]", fillStyle.Color.Script);
        Assert.NotNull(fillStyle.Translate.Value);
        //Assert.Equal("feature[\"translate\"]", fillStyle.Translate.Script);
        Assert.Equal(2, fillStyle.Translate.Value.Length);
        Assert.Equal(1, fillStyle.Translate.Value[0]);
        Assert.Equal(2, fillStyle.Translate.Value[1]);
        Assert.Equal(TranslateAnchor.Viewport, fillStyle.TranslateAnchor.Value);
        // Assert.Equal("feature[\"translateAnchor\"]", fillStyle.TranslateAnchor.Script);
        Assert.Equal("sprite pattern", fillStyle.Pattern.Value);
        Assert.Equal("file://images/colorblocks1.png", fillStyle.Uri.Value);
        // Assert.Equal("feature[\"uri\"]", fillStyle.Uri.Expression);

        Assert.Equal(1, lineStyle.Opacity.Value);
        Assert.Equal(1, lineStyle.Width.Value);
        Assert.Equal("#3ed53e", lineStyle.Color.Value);
        Assert.Equal(2, lineStyle.DashArray.Value.Length);
        Assert.Equal(1.0f, lineStyle.DashArray.Value[0]);
        Assert.Equal(1.0f, lineStyle.DashArray.Value[1]);
        Assert.Equal(10f, lineStyle.DashOffset.Value);
        Assert.Equal("lineJoin string", lineStyle.LineJoin.Value);
        Assert.Equal(2, lineStyle.Translate.Value.Length);
        //Assert.Equal("feature[\"translate\"]", lineStyle.Translate.Script);
        Assert.Equal(1d, lineStyle.Translate.Value[0]);
        Assert.Equal(2d, lineStyle.Translate.Value[1]);
        Assert.Equal(TranslateAnchor.Viewport, lineStyle.TranslateAnchor.Value);
        //Assert.Equal("feature[\"translateAnchor\"]", lineStyle.TranslateAnchor.Script);
        Assert.Equal(1, lineStyle.GapWidth.Value);
        //Assert.Equal("feature[\"gapWidth\"]", lineStyle.GapWidth.Script);
        Assert.Equal(2, lineStyle.Offset.Value);
        //Assert.Equal("feature[\"offset\"]", lineStyle.Offset.Script);
        Assert.Equal(3, lineStyle.Blur.Value);
        //Assert.Equal("feature[\"blur\"]", lineStyle.Blur.Script);
        Assert.Equal(4, lineStyle.Gradient.Value);
        Assert.Equal("sprite pattern", lineStyle.Pattern.Value);
        //Assert.Equal("feature[\"sprite-pattern\"]", lineStyle.Pattern.Script);

        Assert.Equal("property expression", textStyle.Label.Value);
        // Assert.Equal("property expression", textStyle.Label.Expression);
        Assert.Equal("auto", textStyle.Align.Value);
        Assert.Equal(2, textStyle.Font.Value.Count);
        Assert.Equal("Open Sans Regular", textStyle.Font.Value[0]);
        Assert.Equal("Arial Unicode MS Regular", textStyle.Font.Value[1]);
        Assert.Equal("#3ed53e", textStyle.Color.Value);
        Assert.Equal(16, textStyle.Size.Value);
        Assert.Equal(10f, textStyle.Rotate.Value);
        Assert.Equal(TextTransform.None, textStyle.Transform.Value);
        Assert.Equal(2, textStyle.Offset.Value.Length);
        Assert.Equal(90, textStyle.Offset.Value[0]);
        Assert.Equal(90, textStyle.Offset.Value[1]);
    }
}