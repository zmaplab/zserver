using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZMap.SLD;

/// <summary>
/// https://schemas.opengis.net/sld/1.1/StyledLayerDescriptor.xsd
/// A FeatureTypeConstraint identifies a specific feature type and
/// supplies fitlering.
/// </summary>
public class FeatureTypeConstraint : LayerConstraint
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("FeatureTypeName")]
    public string FeatureTypeName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Extent")]
    public List<Extent> Extents { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Filter")]
    public Filter.Filter Filter { get; set; }
}