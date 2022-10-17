using System.Xml.Serialization;

namespace ZMap.SLD
{
    /// <summary>
    /// An "OnlineResource" is typically used to refer to an HTTP URL.
    /// </summary>
    public class OnlineResource
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("xlink")]
        public string Uri { get; set; }

        // public void ReadXml(XmlReader reader)
        // {
        //     if (reader.LocalName == "OnlineResource" && reader.NodeType == XmlNodeType.EndElement)
        //     {
        //         Uri = new Uri(reader.GetAttribute("xlink:href"));
        //     }
        // }
    }
}