namespace ZMap.SLD;

public class AnchorPoint
{
    /// <summary>
    /// 标注X轴均偏移倍数
    /// </summary>
    [XmlElement("AnchorPointX")]
    public float AnchorPointX { get; set; }

    /// <summary>
    /// 标注Y轴均偏移倍数
    /// </summary>
    [XmlElement("AnchorPointY")]
    public float AnchorPointY { get; set; }
}