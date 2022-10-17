using System.Xml;
using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class VendorOption
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlText]
        public string Value { get; set; }
        // public void ReadXml(XmlReader reader)
        // {
        //     var attribute = reader.GetAttribute("name").ToLower();
        //     switch (attribute)
        //     {
        //         case "distance":
        //             {
        //                 Distance = reader.ReadElementContentAsString();
        //                 break;
        //             }
        //     }
        // }
    }
}