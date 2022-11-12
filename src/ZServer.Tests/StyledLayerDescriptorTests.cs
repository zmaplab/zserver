using System.Xml.Serialization;
using Xunit;
using ZMap.SLD;

namespace ZServer.Tests
{
    public class StyledLayerDescriptorTests
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


        [Fact]
        public void ReadFromFile()
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
        }

        [Fact]
        public void GenerateStyle()
        {
            var styledLayerDescriptor = StyledLayerDescriptor.Load("sld/PropertyIsEqualTo.xml");
        }
    }
}