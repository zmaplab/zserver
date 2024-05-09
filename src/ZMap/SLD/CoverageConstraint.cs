namespace ZMap.SLD;

/// <summary>
///  A CoverageConstraint identifies a specific coverage offering
/// and supplies time and range selection.
/// </summary>
public class CoverageConstraint
{
    /// <summary>
    /// Required: false
    /// </summary>
    [XmlElement("Name")]
    public string Name { get; set; }

    [XmlElement("CoverageExtent")]
    public CoverageExtent Extent { get; set; }
}