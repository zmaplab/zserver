namespace ZMap.SLD.Filter;

/// <remarks/>
[XmlRoot("PropertyIsEqualTo")]
public class BinaryComparisonOpType : ComparisonOpsType
{
    /// <remarks/>
    [XmlElement("Add", typeof(Add))]
    [XmlElement("Div", typeof(Div))]
    [XmlElement("Function", typeof(FunctionType1))]
    [XmlElement("Literal", typeof(LiteralType))]
    [XmlElement("Mul", typeof(Mul))]
    [XmlElement("Sub", typeof(Sub))]
    [XmlElement("PropertyName", typeof(PropertyNameType))]
    [XmlChoiceIdentifier("ItemsElementName")]
    public ExpressionType[] Items { get; set; }

    /// <remarks/>
    [XmlElement("ItemsElementName")]
    [XmlIgnore]
    public BinaryComparisonOpChoiceType[] ItemsElementName { get; set; }

    /// <remarks/>
    [XmlAttribute("matchCase")]
    public bool MatchCase { get; set; } = true;

    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        if (Items == null || Items.Length != 2)
        {
            throw new ArgumentException("需要 2 个表达式");
        }

        return null;
    }

    public enum BinaryComparisonOpChoiceType
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