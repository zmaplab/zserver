using System;
using System.Linq;
using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD;

[Serializable]
public class ParameterValue
{
    /// <summary>
    /// 说明： 暂时只支持单个表达式， GeoServer 亦是如此， 后续看为什么对 SLD 的支持不完整
    /// </summary>
    [XmlElement("Add", typeof(BinaryOperatorType))]
    [XmlElement("Div", typeof(BinaryOperatorType))]
    [XmlElement("Function", typeof(FunctionType1))]
    [XmlElement("Literal", typeof(LiteralType))]
    [XmlElement("Mul", typeof(BinaryOperatorType))]
    [XmlElement("PropertyName", typeof(PropertyNameType))]
    [XmlElement("Sub", typeof(BinaryOperatorType))]
    [XmlChoiceIdentifier("ItemsElementName")]
    public ExpressionType[] Items { get; set; }

    /// <remarks/>
    [XmlElement("ItemsElementName")]
    [XmlIgnore]
    public ExpressionChoiceType[] ItemsElementName { get; set; }

    /// <remarks/>
    [XmlText]
    public string[] Text { get; set; }

    // /// <summary>
    // /// 
    // /// </summary>
    // [XmlIgnore]
    // public string Name { get; set; }

    public enum ExpressionChoiceType
    {
        /// <remarks/>
        [XmlEnum] Add,

        /// <remarks/>
        [XmlEnum] Div,

        /// <remarks/>
        [XmlEnum] Function,

        /// <remarks/>
        [XmlEnum] Literal,

        /// <remarks/>
        [XmlEnum] Mul,

        /// <remarks/>
        [XmlEnum] PropertyName,

        /// <remarks/>
        [XmlEnum] Sub
    }

    public virtual object Accept(IStyleVisitor visitor, object extraData)
    {
        if (Text != null)
        {
            visitor.Push(Text);
        }
        else
        {
            foreach (var expressionType in Items)
            {
                visitor.Visit(expressionType, extraData);
            }
        }

        return null;
    }
}

[XmlInclude(typeof(SvgParameter))]
[XmlInclude(typeof(CssParameter))]
public class NamedParameter : ParameterValue
{
    /// <summary>
    /// 
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }

    public override object Accept(IStyleVisitor visitor, object extraData)
    {
        visitor.Push(Text?.ElementAtOrDefault(0));
        return null;
    }

    public enum NamedParameterChoiceType
    {
        /// <remarks/>
        [XmlEnum] SvgParameter,

        /// <remarks/>
        [XmlEnum] CssParameter
    }
}

[XmlRoot("SvgParameter")]
public class SvgParameter : NamedParameter;

[XmlRoot("CssParameter")]
public class CssParameter : NamedParameter;