namespace ZMap.SLD;

public class Normalize
{
    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.LocalName == "Normalize" && reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
        }
    }
}