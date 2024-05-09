namespace ZMap.SLD.Filter.Expression;

/// <remarks/>
[Serializable]
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
        // TODO: 登录反射来注册，不再使用 Switch
        switch (Name)
        {
            // case "ToArray":
            // {
            //     var func = new ToArray(this);
            //     func.Accept(visitor, extraData);
            //     break;
            // }
            // case "ToString":
            // {
            //     var func = new ToString(this);
            //     func.Accept(visitor, extraData);
            //     break;
            // }
            case "Env":
            {
                var func = new Env(this);
                func.Accept(visitor, extraData);
                break;
            }
        }

        return null;
    }

    /// <remarks/>
    [Serializable]
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