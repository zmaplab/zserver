using System.Xml.Serialization;

namespace ZMap.SLD;

[XmlRoot("Mark")]
public class Mark : GraphicalSymbol
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("WellKnownName")]
    public string WellKnownName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Fill")]
    public Fill Fill { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Stroke")]
    public Stroke Stroke { get; set; }
}