namespace ZMap.SLD.Filter.Expression;

/// <remarks/>
[XmlRoot("Add")]
public class BinaryOperatorType : ExpressionType
{
    /// <remarks/>
    [XmlElement("Add", typeof(Add))]
    [XmlElement("Div", typeof(Div))]
    [XmlElement("Function", typeof(FunctionType1))]
    [XmlElement("Literal", typeof(LiteralType))]
    [XmlElement("Mul", typeof(Mul))]
    [XmlElement("PropertyName", typeof(PropertyNameType))]
    [XmlElement("Sub", typeof(Sub))]
    [XmlChoiceIdentifier("ItemsElementName")]
    public ExpressionType[] Items { get; set; }

    /// <remarks/>
    [XmlElement("ItemsElementName")]
    [XmlIgnore]
    public BinaryOperatorItemsChoiceType[] ItemsElementName { get; set; }

    /// <remarks/>
    public enum BinaryOperatorItemsChoiceType
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

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        if (Items == null || Items.Length != 2)
        {
            throw new ArgumentException("需要 2 个表达式");
        }

        return null;
    }
}