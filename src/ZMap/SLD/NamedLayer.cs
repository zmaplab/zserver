using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZMap.SLD
{
    /// <summary>
    /// A NamedLayer is a layer of data that has a name advertised by a WMS.
    /// </summary>
    public class NamedLayer : StyledLayer
    {
        /// <summary>
        /// LayerFeatureConstraints define what features &amp; feature types are
        /// referenced in a layer.
        /// </summary>
        [XmlArray("LayerFeatureConstraints")]
        [XmlArrayItem("FeatureTypeConstraint")]
        public List<FeatureTypeConstraint> FeatureConstraints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "NamedStyle", Type = typeof(NamedStyle))]
        [XmlElement(ElementName = "UserStyle", Type = typeof(UserStyle))]
        public List<Style> Styles { get; set; }

        public NamedLayer()
        {
            // UserStyles = new List<UserStyle>();
        }

        public void Valid()
        {
            // TODO: 校验 NamedStyle 和 UserStyle 只有一种
        }

        public void Accept(IStyleVisitor visitor, object data)
        {
            visitor.Visit(this, data);
        }

        // public void ReadXml(XmlReader reader)
        // {
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "NamedLayer" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else
        //             switch (reader.LocalName)
        //             {
        //                 case "Name" when reader.NodeType == XmlNodeType.Element:
        //                     Name = reader.ReadString();
        //                     break;
        //                 case "UserStyle" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var userStyle = new UserStyle();
        //                         userStyle.ReadXml(reader);
        //                         UserStyles.Add(userStyle);
        //                         break;
        //                     }
        //             }
        //
        //     }
        // }
    }
}