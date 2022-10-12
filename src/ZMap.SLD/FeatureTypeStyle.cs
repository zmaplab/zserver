// using System.Collections.Generic;
// using System.Xml;
//
// namespace ZMap.SLD
// {
//     public class FeatureTypeStyle
//     {
//         public string Name { get; set; }
//         public Description Description { get; set; }
//         public string FeatureTypeName { get; set; }
//         public string[] SemanticTypeIdentifier { get; set; }
//         public List<Rule> Rules { get; private set; }
//         public List<OnlineResource> OnlineResources { get; set; }
//         public VendorOption VendorOption { get; set; }
//
//         public FeatureTypeStyle()
//         {
//             Rules = new List<Rule>();
//             OnlineResources= new List<OnlineResource>();
//         }
//
//         public void ReadXml(XmlReader reader)
//         {
//             var semanticTypeIdentifier = new List<string>();
//             while (reader.Read())
//             {
//                 if (reader.LocalName == "FeatureTypeStyle" && reader.NodeType == XmlNodeType.EndElement)
//                 {
//                     break;
//                 }
//                 else
//                     switch (reader.LocalName)
//                     {
//                         case "Name" when reader.NodeType == XmlNodeType.Element:
//                             Name = reader.ReadString();
//                             break;
//                         case "Description" when reader.NodeType == XmlNodeType.Element:
//                             var description = new Description();
//                             description.ReadXml(reader);
//                             Description = description;
//                             break;
//                         case "FeatureTypeName" when reader.NodeType == XmlNodeType.Element:
//                             FeatureTypeName = reader.ReadString();
//                             break;
//                         case "SemanticTypeIdentifier" when reader.NodeType == XmlNodeType.Element:
//                             var val = reader.ReadString();
//                             semanticTypeIdentifier.Add(val);
//                             break;
//                         case "Rule" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var rule = new Rule();
//                                 rule.ReadXml(reader);
//                                 Rules.Add(rule);
//                                 break;
//                             }
//                         case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var onlineResource = new OnlineResource();
//                                 onlineResource.ReadXml(reader);
//                                 OnlineResources.Add(onlineResource);
//                                 break;
//                             }
//                         case "VendorOption" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var vendorOption = new VendorOption();
//                                 vendorOption.ReadXml(reader);
//                                 VendorOption = vendorOption;
//                                 break;
//                             }
//                     }
//             }
//             SemanticTypeIdentifier = semanticTypeIdentifier.ToArray();
//         }
//     }
// }