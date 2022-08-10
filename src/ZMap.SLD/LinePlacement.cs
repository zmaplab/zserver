using System.Xml;

namespace ZMap.SLD
{
    public class LinePlacement
    {
        public float PerpendicularOffset { get; set; }
        public bool IsRepeated { get; set; }
        public float InitialGap { get; set; }
        public float Gap { get; set; }
        public bool IsAligned { get; set; }
        public bool GeneralizeLine { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "LinePlacement" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "PerpendicularOffset" when reader.NodeType == XmlNodeType.Element:
                            PerpendicularOffset = reader.ReadElementContentAsFloat();
                            break;
                        case "IsRepeated" when reader.NodeType == XmlNodeType.Element:
                            IsRepeated = reader.ReadElementContentAsBoolean();
                            break;
                        case "InitialGap" when reader.NodeType == XmlNodeType.Element:
                            InitialGap = reader.ReadElementContentAsFloat();
                            break;
                        case "InitialGap" when reader.NodeType == XmlNodeType.Element:
                            InitialGap = reader.ReadElementContentAsFloat();
                            break;
                        case "Gap" when reader.NodeType == XmlNodeType.Element:
                            Gap = reader.ReadElementContentAsFloat();
                            break;
                        case "IsAligned" when reader.NodeType == XmlNodeType.Element:
                            IsAligned = reader.ReadElementContentAsBoolean();
                            break;
                        case "GeneralizeLine" when reader.NodeType == XmlNodeType.Element:
                            GeneralizeLine = reader.ReadElementContentAsBoolean();
                            break;
                    }
            }
        }
    }
}