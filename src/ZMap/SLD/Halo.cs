using System.Xml.Serialization;

namespace ZMap.SLD;

public class Halo
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Radius")]
    public float Radius { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Fill Fill { get; set; }
        
    public void Accept(IStyleVisitor visitor, object extraData)
    {
    }
}