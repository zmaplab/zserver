using System.Xml;

namespace ZMap.SLD
{
    public class ExternalGraphic : GraphicalSymbol
    {
        public OnlineResource OnlineResource { get; set; }
        public string Format { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "ExternalGraphic" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
                            var onlineResource = new OnlineResource();
                            // onlineResource.ReadXml(reader);
                            OnlineResource = onlineResource;
                            break;
                        case "Format" when reader.NodeType == XmlNodeType.Element:
                            Format = reader.ReadString();
                            break;
                    }
            }
        }
    }
}