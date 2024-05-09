namespace ZMap.SLD;

public class LinePlacement
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("PerpendicularOffset")]
    public float PerpendicularOffset { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("IsRepeated")]
    public bool IsRepeated { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("InitialGap")]
    public float InitialGap { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Gap")]
    public float Gap { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("IsAligned")]
    public bool IsAligned { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("GeneralizeLine")]
    public bool GeneralizeLine { get; set; }
}