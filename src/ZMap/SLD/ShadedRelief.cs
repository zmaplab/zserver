using System.Xml;

namespace ZMap.SLD
{
    public class ShadedRelief
    {
        public bool BrightnessOnly { get; set; }
        public double ReliefFactor { get; set; }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "ShadedRelief" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "ShadedRelief" when reader.NodeType == XmlNodeType.Element:
                            BrightnessOnly = reader.ReadElementContentAsBoolean();
                            break;
                        case "ReliefFactor" when reader.NodeType == XmlNodeType.Element:
                            ReliefFactor = reader.ReadElementContentAsDouble();
                            break;
                    }
            }
        }
    }
}