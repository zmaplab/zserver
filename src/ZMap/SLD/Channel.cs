using System.Xml;

namespace ZMap.SLD;

public class Channel
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public string SourceChannelName { get; set; }

    /// <summary>
    /// 对比增强
    /// </summary>
    public ContrastEnhancement ContrastEnhancement { get; set; }

    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.LocalName.EndsWith("Channel") && reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
            else
                switch (reader.LocalName)
                {
                    case "SourceChannelName" when reader.NodeType == XmlNodeType.Element:
                        SourceChannelName = reader.ReadElementContentAsString();
                        break;
                    case "ContrastEnhancement" when reader.NodeType == XmlNodeType.Element:
                    {
                        var contrastEnhancement = new ContrastEnhancement();
                        contrastEnhancement.ReadXml(reader);
                        ContrastEnhancement = contrastEnhancement;
                        break;
                    }
                }
        }
    }
}