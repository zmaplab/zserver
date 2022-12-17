using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

/// <remarks/>
[System.SerializableAttribute]
[XmlType(TypeName = "FunctionType")]
[XmlRoot("Function")]
public class FunctionType1 : ExpressionType
{
    /// <remarks/>
    [XmlElement("Add", typeof(Add), Order = 0)]
    [XmlElement("Div", typeof(Div), Order = 0)]
    [XmlElement("Function", typeof(FunctionType1), Order = 0)]
    [XmlElement("Literal", typeof(LiteralType), Order = 0)]
    [XmlElement("Mul", typeof(Mul), Order = 0)]
    [XmlElement("PropertyName", typeof(PropertyNameType), Order = 0)]
    [XmlElement("Sub", typeof(Sub), Order = 0)]
    [XmlChoiceIdentifier("ItemsElementName")]
    public ExpressionType[] Items { get; set; }

    /// <remarks/>
    [XmlElement("ItemsElementName", Order = 1)]
    [XmlIgnore]
    public Function1ItemsChoiceType[] ItemsElementName { get; set; }

    /// <remarks/>
    [XmlAttribute("name")]
    public string Name { get; set; }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        throw new System.NotImplementedException();
    }
    
    /// <remarks/>
    [System.SerializableAttribute]
    [XmlType]
    public enum Function1ItemsChoiceType
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