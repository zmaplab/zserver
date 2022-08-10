using System.Xml;

namespace ZMap.SLD
{
    public class LabelPlacement
    {
        public PointPlacement PointPlacement { get; set; }
        public LinePlacement LinePlacement { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "LabelPlacement" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "PointPlacement" when reader.NodeType == XmlNodeType.Element:
                            {
                                var pointPlacement = new PointPlacement();
                                pointPlacement.ReadXml(reader);
                                PointPlacement = pointPlacement;
                                break;
                            }

                        case "LinePlacement" when reader.NodeType == XmlNodeType.Element:
                            {
                                var linePlacement = new LinePlacement();
                                linePlacement.ReadXml(reader);
                                LinePlacement = linePlacement;
                                break;
                            }
                    }
            }
        }
    }
}