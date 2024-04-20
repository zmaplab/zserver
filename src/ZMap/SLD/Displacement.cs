using System.Xml.Serialization;

namespace ZMap.SLD;

public class Displacement
{
    /// <summary>
    /// 标注在X轴位移的px值
    /// </summary>
    [XmlElement("DisplacementX")]
    public float DisplacementX { get; set; }

    /// <summary>
    /// 标注在Y轴位移的px值
    /// </summary>
    [XmlElement("DisplacementY")]
    public float DisplacementY { get; set; }
}