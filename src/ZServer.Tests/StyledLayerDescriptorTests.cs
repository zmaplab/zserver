// using System.Xml.Serialization;
// using OpenGIS.Style.SE;
// using Xunit;
//
// namespace ZServer.Tests
// {
//     public class StyledLayerDescriptorTests
//     {
//         const string XML = @"<?xml version=""1.0""?>
//                         <DietPlan>
//                             <Health>
//                                 <Fruit>Test</Fruit>
//                                 <Fruit>Test</Fruit>
//                                 <Veggie>Test</Veggie>
//                                 <Veggie>Test</Veggie>
//                             </Health>
//                         </DietPlan>";
//
//         [XmlRoot(ElementName = "DietPlan")]
//         public class TestSerialization
//         {
//             [XmlArray("Health")]
//             [XmlArrayItem("Fruit", Type = typeof(Fruit))]
//             [XmlArrayItem("Veggie", Type = typeof(Veggie))]
//             public Food[] Foods { get; set; }
//         }
//
//         [XmlInclude(typeof(Fruit))]
//         [XmlInclude(typeof(Veggie))]
//         public class Food
//         {
//             [XmlText]
//             public string Text { get; set; }
//         }
//
//         public class Fruit : Food { }
//         public class Veggie : Food { }
//  
//         
//         [Fact]
//         public void ReadFromFile()
//         {
//             var styledLayerDescriptor1 = StyledLayerDescriptor.Load("se.sld");
//             Assert.Equal("DI_规划道路中线", styledLayerDescriptor1.NamedLayers[0].Name);
//             Assert.Equal("NAME1", styledLayerDescriptor1.NamedLayers[1].Name);
//             // var styledLayerDescriptor2 = StyledLayerDescriptor.Load("sld.sld");
//             // Assert.Equal("usa:states", styledLayerDescriptor2.NamedLayers[0].Name);
//         }
//     }
// }