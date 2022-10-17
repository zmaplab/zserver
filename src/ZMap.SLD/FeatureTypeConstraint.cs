using System.Xml.Serialization;

namespace ZMap.SLD;

/// <summary>
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
    public Extent[] Extents { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Filter")]
    public Filter.Filter Filter { get; set; }
}