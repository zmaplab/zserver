namespace ZMap.SLD.Filter.Expression;

/// <remarks/>
// [XmlIncludeAttribute(typeof(RecodeType))]
// [XmlIncludeAttribute(typeof(InterpolateType))]
// [XmlIncludeAttribute(typeof(CategorizeType))]
// [XmlIncludeAttribute(typeof(StringLengthType))]
// [XmlIncludeAttribute(typeof(StringPositionType))]
[XmlInclude(typeof(TrimType))]
// [XmlIncludeAttribute(typeof(ChangeCaseType))]
// [XmlIncludeAttribute(typeof(ConcatenateType))]
// [XmlIncludeAttribute(typeof(SubstringType))]
// [XmlIncludeAttribute(typeof(FormatDateType))]
// [XmlIncludeAttribute(typeof(FormatNumberType))]
[Serializable]
[XmlType(Namespace = "http://www.opengis.net/ogc")]
public abstract class FunctionType : ExpressionType
{
    /// <remarks/>
    [XmlAttribute("fallbackValue")]
    public string FallbackValue { get; set; }
}