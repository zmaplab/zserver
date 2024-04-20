using System.Xml;

namespace ZMap.SLD;

public class ChannelSelection
{
    /// <summary>
    /// 红色通道
    /// </summary>
    public RedChannel RedChannel { get; set; }
    /// <summary>
    /// 绿色通道
    /// </summary>
    public GreenChannel GreenChannel { get; set; }
    /// <summary>
    /// 蓝色通道
    /// </summary>
    public BlueChannel BlueChannel { get; set; }
    /// <summary>
    /// 灰度通道
    /// </summary>
    public GrayChannel GrayChannel { get; set; }
    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.LocalName.EndsWith("ChannelSelection") && reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
            else
                switch (reader.LocalName)
                {
                    case "RedChannel" when reader.NodeType == XmlNodeType.Element:
                        var redChannel = new RedChannel();
                        redChannel.ReadXml(reader);
                        RedChannel = redChannel;
                        break;
                    case "GreenChannel" when reader.NodeType == XmlNodeType.Element:
                        var greenChannel = new GreenChannel();
                        greenChannel.ReadXml(reader);
                        GreenChannel = greenChannel;
                        break;
                    case "RedChannel" when reader.NodeType == XmlNodeType.Element:
                        var blueChannel = new BlueChannel();
                        blueChannel.ReadXml(reader);
                        BlueChannel = blueChannel;
                        break;
                    case "RedChannel" when reader.NodeType == XmlNodeType.Element:
                        var grayChannel = new GrayChannel();
                        grayChannel.ReadXml(reader);
                        GrayChannel = grayChannel;
                        break;
                }
        }
    }
}