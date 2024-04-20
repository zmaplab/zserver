using System.Xml;

namespace ZMap.SLD;

public class ContrastEnhancement
{
    public Histogram Histogram { get; set; }
    public Normalize Normalize { get; set; }
    public float GammaValue { get; set; }
    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.LocalName == "ContrastEnhancement" && reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
            else
                switch (reader.LocalName)
                {
                    case "Histogram" when reader.NodeType == XmlNodeType.Element:
                    {
                        var histogram = new Histogram();
                        histogram.ReadXml(reader);
                        Histogram = histogram;
                        break;
                    }

                    case "Normalize" when reader.NodeType == XmlNodeType.Element:
                    {
                        var normalize = new Normalize();
                        normalize.ReadXml(reader);
                        Normalize = normalize;
                        break;
                    }

                    case "GammaValue" when reader.NodeType == XmlNodeType.Element:
                        GammaValue = reader.ReadContentAsFloat();
                        break;
                }
        }
    }

}