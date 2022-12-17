using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("FeatureId")]
public class FeatureIdType : AbstractIdType
{
    /// <remarks/>
    [XmlAttribute(DataType = "ID")]
    public string FId { get; set; }
}