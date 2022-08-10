using System.Xml;

namespace ZMap.SLD
{
    public class PointPlacement
    {
        public AnchorPoint AnchorPoint { get; set; }
        public Displacement Displacement { get; set; } = new Displacement(0, 0);
        public int Rotation { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "PointPlacement" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "AnchorPoint" when reader.NodeType == XmlNodeType.Element:
                            {
                                var anchorPoint = new AnchorPoint();
                                anchorPoint.ReadXml(reader);
                                AnchorPoint = anchorPoint;
                                break;
                            }

                        case "Displacement" when reader.NodeType == XmlNodeType.Element:
                            {
                                var displacement = new Displacement();
                                displacement.ReadXml(reader);
                                Displacement = displacement;
                                break;
                            }

                        case "Rotation" when reader.NodeType == XmlNodeType.Element:
                            Rotation = reader.ReadContentAsInt();
                            break;
                    }
            }
        }
    }
}