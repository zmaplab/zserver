namespace ZMap.SLD.Filter;

/// <remarks/>
[Serializable]
[XmlType]
public class UpperBoundaryType
{
    /// <remarks/>
    [XmlElement("Add", typeof(Add))]
    [XmlElement("Div", typeof(Div))]
    [XmlElement("Function", typeof(FunctionType1))]
    [XmlElement("Literal", typeof(LiteralType))]
    [XmlElement("Mul", typeof(Mul))]
    [XmlElement("PropertyName", typeof(PropertyNameType))]
    [XmlElement("Sub", typeof(Sub))]
    [XmlChoiceIdentifier("ItemElementName")]
    public ExpressionType Item { get; set; }

    /// <remarks/>
    [XmlIgnore]
    [XmlElement("ItemElementName")]
    public UpperBoundaryItemChoiceType ItemElementName { get; set; }

    public virtual object Accept(IFilterVisitor visitor, object extraData)
    {
        throw new NotImplementedException();
    }

    /// <remarks/>
    [Serializable]
    [XmlType]
    public enum UpperBoundaryItemChoiceType
    {
        /// <remarks/>
        Add,

        /// <remarks/>
        Div,

        /// <remarks/>
        Function,

        /// <remarks/>
        Literal,

        /// <remarks/>
        Mul,

        /// <remarks/>
        PropertyName,

        /// <remarks/>
        Sub,
    }
}