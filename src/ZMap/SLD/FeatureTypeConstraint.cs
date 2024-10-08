using ZMap.SLD.Filter;

namespace ZMap.SLD;

/// <summary>
/// https://schemas.opengis.net/sld/1.1/StyledLayerDescriptor.xsd
/// A FeatureTypeConstraint identifies a specific feature type and
/// supplies fitlering.
/// </summary>
public class FeatureTypeConstraint : LayerConstraint, IStyle
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
    public FilterType Filter { get; set; }

    public virtual object Accept(IStyleVisitor visitor, object data)
    {
        return visitor.Visit(this, data);
    }
}