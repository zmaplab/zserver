namespace ZMap.SLD.Filter.Expression;

/// <remarks/>
[Serializable]
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
    [Serializable]
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
        throw new NotImplementedException();
    }
}