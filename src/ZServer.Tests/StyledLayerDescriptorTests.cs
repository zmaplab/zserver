using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap;
using ZMap.SLD;
using ZMap.Style;

namespace ZServer.Tests
{
    public class StyledLayerDescriptorTests : BaseTests
    {
        const string XML = @"<?xml version=""1.0""?>
                        <DietPlan>
                            <Health>
                                <Fruit>Test</Fruit>
                                <Fruit>Test</Fruit>
                                <Veggie>Test</Veggie>
                                <Veggie>Test</Veggie>
                            </Health>
                        </DietPlan>";

        [XmlRoot(ElementName = "DietPlan")]
        public class TestSerialization
        {
            [XmlArray("Health")]
            [XmlArrayItem("Fruit", Type = typeof(Fruit))]
            [XmlArrayItem("Veggie", Type = typeof(Veggie))]
            public Food[] Foods { get; set; }
        }

        [XmlInclude(typeof(Fruit))]
        [XmlInclude(typeof(Veggie))]
        public class Food
        {
            [XmlText] public string Text { get; set; }
        }

        public class Fruit : Food
        {
        }

        public class Veggie : Food
        {
        }


        [Fact(Skip = "SLD 实现验证")]
        public void LoadFromXml()
        {
            var styledLayerDescriptor1 = StyledLayerDescriptor.Load("se.xml");
            Assert.Equal("StyledLayerDescriptor1", styledLayerDescriptor1.Name);
            Assert.Equal("1.1.0", styledLayerDescriptor1.Version);
            Assert.Equal("StyledLayerDescriptor_Title", styledLayerDescriptor1.Description.Title);
            Assert.Equal("StyledLayerDescriptor_Abstract", styledLayerDescriptor1.Description.Abstract);

            Assert.Equal("DI_规划道路中线", styledLayerDescriptor1.Layers[0].Name);
            Assert.Equal("NAME1", styledLayerDescriptor1.Layers[1].Name);

            var namedLayer = styledLayerDescriptor1.Layers[0] as NamedLayer;
            Assert.NotNull(namedLayer);

            Assert.Equal("FeatureTypeName1", namedLayer.FeatureConstraints[0].FeatureTypeName);
            Assert.Equal("extent1", namedLayer.FeatureConstraints[0].Extents[0].Name);
            Assert.Equal("extent_value1", namedLayer.FeatureConstraints[0].Extents[0].Value);

            var userStyle1 = namedLayer.Styles[0] as UserStyle;
            Assert.NotNull(userStyle1);
            Assert.Equal("DI_规划道路中线", userStyle1.Name);

            var featureTypeStyle = userStyle1.FeatureTypeStyles[0];
            Assert.NotNull(featureTypeStyle);

            var vendorOption1 = featureTypeStyle.VendorOptions[0];
            Assert.NotNull(vendorOption1);
            Assert.Equal("option1", vendorOption1.Name);
            Assert.Equal("option_value1", vendorOption1.Value);

            var rules = featureTypeStyle.Rules;
            Assert.Equal(10, rules.Count);

            var rule1 = rules[0];
            Assert.Equal("规划道路中线L16", rule1.Name);
            Assert.True(rule1.ElseFilter);
            Assert.Equal(10000000d, rule1.MaxScaleDenominator);
            Assert.Equal(6772d, rule1.MinScaleDenominator);

            var pointSymbolizer1 = (PointSymbolizer)rule1.Symbolizers[3];
            // var a = pointSymbolizer1.Graphic.Rotation[0].Accept(new DefaultFilterVisitor(), null);
            var size = pointSymbolizer1.Graphic.Size;
            Assert.Equal("14", size.Text[0]);
            // Assert.Equal("14", pointSymbolizer1.Graphic.Rotation);
            var rule2 = rules[1];
            Assert.Equal("规划道路中线L17", rule2.Name);
            Assert.False(rule2.ElseFilter);
            Assert.Equal(6771d, rule2.MaxScaleDenominator);
            Assert.Equal(3387d, rule2.MinScaleDenominator);

            Assert.Equal(4, rule1.Symbolizers.Count);

            var symbolizer1 = rule1.Symbolizers[0];
            Assert.IsType<LineSymbolizer>(symbolizer1);
            var lineSymbolizer1 = (LineSymbolizer)symbolizer1;
            var stroke1 = lineSymbolizer1.Stroke;
            // Assert.Equal("#f8f8f8", stroke1.Color.Text);
            Assert.Equal("2", stroke1.Width.Text[0]);
            Assert.Null(stroke1.Opacity);
            Assert.Equal("round", stroke1.LineCap.Text[0]);
            Assert.Equal("round", stroke1.LineJoin.Text[0]);
            Assert.Null(stroke1.DashArray);
            Assert.Null(stroke1.DashOffset);

            var lineSymbolizer2 = (LineSymbolizer)rule1.Symbolizers[1];
            var stroke2 = lineSymbolizer2.Stroke;
            // Assert.Equal("#dd302d", stroke2.Color.Text);
            Assert.Equal("1", stroke2.Width.Text[0]);
            Assert.Null(stroke2.Opacity);
            Assert.Equal("round", stroke2.LineCap.Text[0]);
            Assert.Equal("round", stroke2.LineJoin.Text[0]);
            Assert.Equal("4 2", stroke2.DashArray.Text[0]);
            Assert.Null(stroke2.DashOffset);

            // var lineSymbolizer3 = (LineSymbolizer)rule1.Symbolizers[2];
            // var stroke3 = lineSymbolizer3.Stroke;
            // // Assert.Equal("#dd302d", stroke2.Color.Text);
            // Assert.Equal("1", stroke2.Width.Text[0]);
            // Assert.Null(stroke2.Opacity);
            // Assert.Equal("round", stroke2.LineCap.Text[0]);
            // Assert.Equal("round", stroke2.LineJoin.Text[0]);
            // Assert.Equal("4 2", stroke2.DashArray.Text[0]);
            // Assert.Null(stroke2.DashOffset);

            var ruleFont1 = rules.First(x => x.Name == "文字1");
            var textSymbolizer1 = (TextSymbolizer)ruleFont1.Symbolizers[0];
            Assert.Equal("微软雅黑", textSymbolizer1.Font.GetOrDefault("font-family").Text[0]);
            Assert.Equal("13", textSymbolizer1.Font.GetOrDefault("font-size").Text[0]);
            Assert.Equal("100", textSymbolizer1.Font.GetOrDefault("font-weight").Text[0]);
            Assert.Equal("style1", textSymbolizer1.Font.GetOrDefault("font-style").Text[0]);
            Assert.Equal("dlmc", textSymbolizer1.Label.PropertyName);
            Assert.Equal(2, textSymbolizer1.Halo.Radius);
            Assert.True(textSymbolizer1.LabelPlacement.LinePlacement.GeneralizeLine);

            var rulePolygon1 = rules.First(x => x.Name == "Polygon1");
            var polygonSymbolizer1 = (PolygonSymbolizer)rulePolygon1.Symbolizers[0];
            // Assert.Equal("#FF0000", polygonSymbolizer1.Stroke.Color.Text);
            Assert.Equal("2", polygonSymbolizer1.Stroke.Width.Text[0]);
            Assert.Equal("#200000", polygonSymbolizer1.Fill.Color.Text[0]);

            var visitor = new SldStyleVisitor();
            visitor.Visit(styledLayerDescriptor1, null);
            var styleGroups = visitor.StyleGroups;
            Assert.Equal(10, styleGroups.Count);
            Assert.Equal(4, styleGroups[0].Styles.Count);

            var style1 = (LineStyle)styleGroups[0].Styles[1];
            Assert.Equal(4, style1.DashArray.Value[0]);
            Assert.Equal(2, style1.DashArray.Value[1]);

            var styleVisitor = new ZMapStyleVisitor();
            foreach (var styleGroup in styleGroups)
            {
                styleGroup.Accept(styleVisitor, new Feature(new Point(1, 1), new Dictionary<string, dynamic>
                {
                    { "dasharray", "14 22" },
                    { "COMID", 14444D }
                }));
            }

            var style3 = (LineStyle)styleGroups[0].Styles[2];
            Assert.Equal(14, style3.DashArray.Value[0]);
            Assert.Equal(22, style3.DashArray.Value[1]);
        }

        [Fact]
        public void GenerateStyle()
        {
            StyledLayerDescriptor.Load("sld/PropertyIsEqualTo.xml");
        }
    }
}