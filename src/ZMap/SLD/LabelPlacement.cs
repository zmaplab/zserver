using System.Xml.Serialization;

namespace ZMap.SLD;

public class LabelPlacement
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("PointPlacement")]
    public PointPlacement PointPlacement { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("LinePlacement")]
    public LinePlacement LinePlacement { get; set; }
}