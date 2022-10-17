using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace ZMap.SLD
{
    /// <summary>
    /// A FeatureTypeStyle contains styling information specific to one
    /// feature type.
    /// </summary>
    public class FeatureTypeStyle
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Description")]
        public Description Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("FeatureTypeName")]
        public string FeatureTypeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("SemanticTypeIdentifier")]
        public List<string> SemanticTypeIdentifiers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("Version")]
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Rule")]
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("OnlineResource")]
        public List<OnlineResource> OnlineResources { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("VendorOption")]
        public List<VendorOption> VendorOptions { get; set; }

        // public FeatureTypeStyle()
        // {
        //     Rules = new List<Rule>();
        //     OnlineResources= new List<OnlineResource>();
        // }

        // public void ReadXml(XmlReader reader)
        // {
        //     var semanticTypeIdentifier = new List<string>();
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "FeatureTypeStyle" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else
        //             switch (reader.LocalName)
        //             {
        //                 case "Name" when reader.NodeType == XmlNodeType.Element:
        //                     Name = reader.ReadString();
        //                     break;
        //                 case "Description" when reader.NodeType == XmlNodeType.Element:
        //                     var description = new Description();
        //                     description.ReadXml(reader);
        //                     Description = description;
        //                     break;
        //                 case "FeatureTypeName" when reader.NodeType == XmlNodeType.Element:
        //                     FeatureTypeName = reader.ReadString();
        //                     break;
        //                 case "SemanticTypeIdentifier" when reader.NodeType == XmlNodeType.Element:
        //                     var val = reader.ReadString();
        //                     semanticTypeIdentifier.Add(val);
        //                     break;
        //                 case "Rule" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var rule = new Rule();
        //                         rule.ReadXml(reader);
        //                         Rules.Add(rule);
        //                         break;
        //                     }
        //                 case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var onlineResource = new OnlineResource();
        //                         onlineResource.ReadXml(reader);
        //                         OnlineResources.Add(onlineResource);
        //                         break;
        //                     }
        //                 case "VendorOption" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var vendorOption = new VendorOption();
        //                         vendorOption.ReadXml(reader);
        //                         VendorOption = vendorOption;
        //                         break;
        //                     }
        //             }
        //     }
        //     SemanticTypeIdentifier = semanticTypeIdentifier.ToArray();
        // }
    }
}