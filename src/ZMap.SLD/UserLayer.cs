using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZMap.SLD;

public class UserLayer : StyledLayer
{
    /// <summary>
    /// LayerFeatureConstraints define what features &amp; feature types are
    /// referenced in a layer.
    /// </summary>
    [XmlArray("LayerFeatureConstraints")]
    [XmlArrayItem("FeatureTypeConstraint")]
    public List<FeatureTypeConstraint> FeatureConstraints { get; set; }

    /// <summary>
    /// LayerCoverageConstraints define what coverage offering subsets
    /// are referenced in a layer.
    /// </summary>
    [XmlArray("LayerCoverageConstraints")]
    [XmlArrayItem("CoverageConstraint")]
    public List<CoverageConstraint> CoverageConstraints { get; set; }
}