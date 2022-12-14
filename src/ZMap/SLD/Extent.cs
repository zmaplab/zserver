using System.Xml.Serialization;

namespace ZMap.SLD;

/// <summary>
/// https://schemas.opengis.net/sld/1.1/StyledLayerDescriptor.xsd
/// An Extent gives feature/coverage/raster/matrix dimension extent.
/// </summary>
public class Extent
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Name")]
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Value")]
    public string Value { get; set; }
}