using System.Xml;

namespace ZMap.SLD;

public class ColorMap
{
    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.LocalName == "ColorMap" && reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
            else
                switch (reader.LocalName)
                {
                    case "" when reader.NodeType == XmlNodeType.Element:
                        break;
                }
        }
    }
}