namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
[XmlType]
[XmlRoot("FeatureId")]
public class FeatureIdType : AbstractIdType
{
    /// <remarks/>
    [XmlAttribute(DataType = "ID")]
    public string FId { get; set; }
}