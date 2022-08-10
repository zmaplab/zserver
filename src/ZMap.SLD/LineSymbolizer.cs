using System.Xml;

namespace ZMap.SLD
{
    public class LineSymbolizer : ISymbolizer
    {
        public OnlineResource OnlineResource { get; set; }
        public Geometry Geometry { get; set; }
        public Stroke Stroke { get; set; }
        public PerpendicularOffset PerpendicularOffset { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "LineSymbolizer" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
                            {
                                var onlineResource = new OnlineResource();
                                onlineResource.ReadXml(reader);
                                OnlineResource = onlineResource;
                                break;
                            }
                        case "Geometry" when reader.NodeType == XmlNodeType.Element:
                            {
                                var geometry = new Geometry();
                                geometry.ReadXml(reader);
                                Geometry = geometry;
                                break;
                            }

                        case "Stroke" when reader.NodeType == XmlNodeType.Element:
                            {
                                var stroke = new Stroke();
                                stroke.ReadXml(reader);
                                Stroke = stroke;
                                break;
                            }

                        case "PerpendicularOffset" when reader.NodeType == XmlNodeType.Element:
                            {
                                var perpendicular = new PerpendicularOffset();
                                perpendicular.ReadXml(reader);
                                PerpendicularOffset = perpendicular;
                                break;
                            }
                    }
            }
        }
    }
}