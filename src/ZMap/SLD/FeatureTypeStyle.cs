using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZMap.SLD;

/// <summary>
/// A FeatureTypeStyle contains styling information specific to one
/// feature type.
/// </summary>
public class FeatureTypeStyle
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Name")]
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Description")]
    public Description Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("FeatureTypeName")]
    public string FeatureTypeName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("SemanticTypeIdentifier")]
    public List<string> SemanticTypeIdentifiers { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlAttribute("Version")]
    public string Version { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Rule")]
    public List<Rule> Rules { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("OnlineResource")]
    public List<OnlineResource> OnlineResources { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("VendorOption")]
    public List<VendorOption> VendorOptions { get; set; }

    // public FeatureTypeStyle()
    // {
    //     Rules = new List<Rule>();
    //     OnlineResources= new List<OnlineResource>();
    // }

    public void Valid()
    {
        // TODO: Rule 和 OnlineResource 只能二选一
    }

    public void Accept(IStyleVisitor styleVisitor, object data)
    {
        styleVisitor.Visit(this, data);
    }
}