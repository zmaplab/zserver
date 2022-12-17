using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("Trim")]
public class TrimType : FunctionType
{
    /// <remarks/>
    [XmlElement(Order = 0)]
    public ParameterValue StringValue { get; set; }

    /// <remarks/>
    [XmlAttribute("stripOffPosition")]
    public StripOffPositionType StripOffPosition { get; set; }

    /// <remarks/>
    [XmlIgnore]
    public bool StripOffPositionSpecified { get; set; }


    /// <remarks/>
    [XmlAttribute("stripOffChar")]
    public string StripOffChar { get; set; }

    /// <remarks/>
    [System.SerializableAttribute]
    [XmlType]
    public enum StripOffPositionType
    {
        /// <remarks/>
        leading,

        /// <remarks/>
        trailing,

        /// <remarks/>
        both,
    }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        throw new System.NotImplementedException();
    }
}