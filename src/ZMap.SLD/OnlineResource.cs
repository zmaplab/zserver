using System;
using System.Xml;

namespace ZMap.SLD
{
    public class OnlineResource
    {
        public Uri Uri { get; set; }
        public void ReadXml(XmlReader reader)
        {
            if (reader.LocalName == "OnlineResource" && reader.NodeType == XmlNodeType.EndElement)
            {
                Uri = new Uri(reader.GetAttribute("xlink:href"));
            }
        }
    }
}