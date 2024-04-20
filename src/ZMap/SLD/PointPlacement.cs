using System.Xml.Serialization;

namespace ZMap.SLD;

public class PointPlacement
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("AnchorPoint")]
    public AnchorPoint AnchorPoint { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Displacement")]
    public Displacement Displacement { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Rotation")]
    public int Rotation { get; set; }
}