using System.Xml;

namespace ZMap.SLD;

public class Histogram
{
    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.LocalName == "Histogram" && reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
        }
    }
}