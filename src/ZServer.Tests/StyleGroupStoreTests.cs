using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap;
using ZMap.Style;
using ZServer.Store;

namespace ZServer.Tests
{
    public class StyleGroupStoreTests : BaseTests
    {
        [Fact]
        public async Task FindByName()
        {
            var store = GetScopedService<IStyleGroupStore>();
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
            Assert.True(styleGroup.Styles[1] is SpriteLineStyle);
            Assert.True(styleGroup.Styles[2] is TextStyle);
            var fillStyle = (SpriteFillStyle)styleGroup.Styles[0];
            var lineStyle = (SpriteLineStyle)styleGroup.Styles[1];
            var textStyle = (TextStyle)styleGroup.Styles[2];

            fillStyle.Accept(new ZMapStyleVisitor(), new Feature(new Point(1, 1), new Dictionary<string, dynamic>
            {
                { "height", 11 }
            }));
            Assert.Equal(true, fillStyle.Filter.Value);
            Assert.Equal(null, lineStyle.Filter.Value);
            Assert.Equal(false, textStyle.Filter.Value);
            Assert.True(fillStyle.Antialias);
            Assert.Equal(1, fillStyle.Opacity.Value);
            Assert.Equal("feature[\"opacity\"]", fillStyle.Opacity.Body);
            Assert.Equal("#3ed53e", fillStyle.Color.Value);
            Assert.Equal("feature[\"color\"]", fillStyle.Color.Body);
            Assert.NotNull(fillStyle.Translate.Value);
            Assert.Equal("feature[\"translate\"]", fillStyle.Translate.Body);
            Assert.Equal(2, fillStyle.Translate.Value.Length);
            Assert.Equal(1, fillStyle.Translate.Value[0]);
            Assert.Equal(2, fillStyle.Translate.Value[1]);
            Assert.Equal(TranslateAnchor.Viewport, fillStyle.TranslateAnchor.Value);
            Assert.Equal("feature[\"translateAnchor\"]", fillStyle.TranslateAnchor.Body);
            Assert.Equal("sprite pattern", fillStyle.Pattern.Value);
            Assert.Equal(new Uri("file://images/colorblocks1.png").AbsoluteUri, fillStyle.Uri.Value.AbsoluteUri);
            Assert.Equal("feature[\"uri\"]", fillStyle.Uri.Body);

            Assert.Equal(1, lineStyle.Opacity.Value);
            Assert.Equal(1, lineStyle.Width.Value);
            Assert.Equal("#3ed53e", lineStyle.Color.Value);
            Assert.Equal(2, lineStyle.DashArray.Value.Length);
            Assert.Equal(1.0f, lineStyle.DashArray.Value[0]);
            Assert.Equal(1.0f, lineStyle.DashArray.Value[1]);
            Assert.Equal(10f, lineStyle.DashOffset.Value);
            Assert.Equal("lineJoin string", lineStyle.LineJoin.Value);
            Assert.Equal(2, lineStyle.Translate.Value.Length);
            Assert.Equal("feature[\"translate\"]", lineStyle.Translate.Body);
            Assert.Equal(1d, lineStyle.Translate.Value[0]);
            Assert.Equal(2d, lineStyle.Translate.Value[1]);
            Assert.Equal(TranslateAnchor.Viewport, lineStyle.TranslateAnchor.Value);
            Assert.Equal("feature[\"translateAnchor\"]", lineStyle.TranslateAnchor.Body);
            Assert.Equal(1, lineStyle.GapWidth.Value);
            Assert.Equal("feature[\"gapWidth\"]", lineStyle.GapWidth.Body);
            Assert.Equal(2, lineStyle.Offset.Value);
            Assert.Equal("feature[\"offset\"]", lineStyle.Offset.Body);
            Assert.Equal(3, lineStyle.Blur.Value);
            Assert.Equal("feature[\"blur\"]", lineStyle.Blur.Body);
            Assert.Equal(4, lineStyle.Gradient.Value);
            Assert.Equal("sprite pattern", lineStyle.Pattern.Value);
            Assert.Equal("feature[\"sprite-pattern\"]", lineStyle.Pattern.Body);

            Assert.Equal("property string", textStyle.Label.Value);
            Assert.Equal("property expression", textStyle.Label.Body);
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
}